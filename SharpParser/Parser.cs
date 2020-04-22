using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpParser
{
    public class Parser
    {
        private string[] Args { get; }

        private bool Valid { get; set; }

        public string ErrorState { get; set; }

        private object Options { get; set; }

        public Parser(string[] args)
        {
            Args = args;
            Valid = true;
        }

        public Parser ParseWith<T>() where T : class, new()
        {
            var opts = new T();
            var props = GetFullPropertyInfo(typeof(T));
            var args = GetNormalizedArgs().ToList();

            var optVal = new Dictionary<PropertyInfo, object>();
            var help = false;

            // handle optional arguments
            foreach (var p in props.Where(prop => !prop.Option.Required))
            {
                var present = p.Aliases.Any(alias => args.Contains(alias));
                if (!present)
                {
                    if (p.PropInfo.PropertyType == typeof(bool))
                    {
                        optVal.Add(p.PropInfo, false);
                    }
                    else
                    {
                        optVal.Add(p.PropInfo, null);
                    }
                }
                else
                {
                    if (p.Option.Name == "help") help = true;
                    var argIndex = args.FindIndex(a => p.Aliases.Any(alias => alias.Contains(a)));
                    if (p.PropInfo.PropertyType == typeof(bool))
                    {
                        optVal.Add(p.PropInfo, true);
                        args.RemoveAt(argIndex);
                    }
                    else
                    {
                        var argVal = args[argIndex + 1];
                        var convertedVal = Convert.ChangeType(argVal, p.PropInfo.PropertyType);
                        optVal.Add(p.PropInfo, convertedVal);
                        args.RemoveRange(argIndex, 2);
                    }
                }
            }

            if (!help)
            {
                // handle required arguments
                foreach (var p in props.Where(prop => prop.Option.Required))
                {
                    var present = p.Aliases.Any(alias => args.Contains(alias));
                    if (!present)
                    {
                        Valid = false;
                        ErrorState = $"Missing argument: {p.Option.Name}";
                        return this;
                    }

                    var argIndex = args.FindIndex(a => p.Aliases.Any(alias => alias.Contains(a)));
                    var argValue = args[argIndex + 1];
                    var convertedVal = Convert.ChangeType(argValue, p.PropInfo.PropertyType);
                    optVal.Add(p.PropInfo, convertedVal);
                    args.RemoveRange(argIndex, 2);
                }
            }

            

            

            foreach (var (prop, val) in optVal)
            {
                prop.SetValue(opts, val);
            }

            Options = opts;
            return this;
        }

        public Parser OnError(Action<string> handler)
        {
            if (Valid) return this;

            handler(ErrorState);
            return this;
        }

        public T RetrieveOptions<T>() where T : class
        {
            if (Valid) return (T) Options;
            return null;
        }

        private IEnumerable<string> GetNormalizedArgs()
        {
            var normalizedArgs = new List<string>();
            foreach (var arg in Args)
            {
                if (arg.Substring(0, 2) == "--") normalizedArgs.Add(arg.TrimStart('-').ToUpper());
                else if (arg[0] == '-')
                {
                    
                    var trimmed = arg.TrimStart('-');
                    if (trimmed.Length > 1)
                    {
                        normalizedArgs.AddRange(trimmed.ToCharArray().Select(c => c.ToString().ToUpper()));
                    }
                    else normalizedArgs.Add(trimmed.ToUpper());
                }
                else normalizedArgs.Add(arg);
            }

            return normalizedArgs;
        }

        private static List<Property> GetFullPropertyInfo(Type classType)
        {
            var propsAttrs = new List<Property>();
            foreach (var prop in classType.GetProperties())
            {
                var attr = (Option)Attribute.GetCustomAttribute(prop, typeof(Option));
                var aliases = new List<string> {attr.Name.ToUpper(), attr.Alias.ToUpper()};
                propsAttrs.Add(new Property
                {
                    Aliases = aliases,
                    Option = attr,
                    PropInfo = prop
                });
            }
            return propsAttrs;
        }

        public static string ConstructBasicManual<T>()
        {
            var properties = GetFullPropertyInfo(typeof(T));

            return properties.Aggregate("Options:\n", (current, property) => current + $"{(property.Option.Required ? "*" : string.Empty)}--{property.Option.Name} -{property.Option.Alias}\t\t {property.Option.Help}\n");
        }

        private struct Property
        {
            public List<string> Aliases { get; set; }
            
            public Option Option { get; set; }

            public PropertyInfo PropInfo { get; set; }
        }
    }
}
