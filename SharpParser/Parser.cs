using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpParser
{
    public class Parser
    {
        #region Properties

        private string[] Args { get; }

        private bool Valid { get; set; }

        public string ErrorState { get; set; }

        private object Options { get; set; }

        #endregion

        #region Constructor

        public Parser(string[] args)
        {
            Args = args;
            Valid = true;
            ErrorState = string.Empty;
            Options = null;
        }

        #endregion

        #region ParserFunctionality

        public Parser ParseWith<T>() where T : class, new()
        {
            var opts = new T();
            var props = GetFullPropertyInfo(typeof(T));
            var args = GetNormalizedArgs().ToList();
            var optVal = new Dictionary<Property, object>();
            var help = false;

            // handle optional arguments
            foreach (var p in props.Where(prop => !prop.Option.Required))
            {
                var present = p.Aliases.Any(alias => args.Contains(alias));
                if (!present)
                {
                    if (p.PropInfo.PropertyType == typeof(bool))
                    {
                        optVal.Add(p, false);
                    }
                    else
                    {
                        optVal.Add(p, null);
                    }
                }
                else
                {
                    if (p.Option.Name == "help") help = true;
                    // check if any other group arguments are not already applied. If there are any throw error
                    var opt = optVal.Keys.Where(o => o.Option.Group.Equals(p.Option.Group));
                    var properties = opt as Property[] ?? opt.ToArray();
                    if (properties.Length > 0)
                    {
                        ErrorState = $"You cant use -{p.Option.Alias} with -{properties.ElementAt(0).Option.Alias}";
                        Valid = false;
                        return this;
                    }
                    var argIndex = args.FindIndex(a => p.Aliases.Any(alias => alias.Contains(a)));
                    if (p.PropInfo.PropertyType == typeof(bool))
                    {
                        optVal.Add(p, true);
                        args.RemoveAt(argIndex);
                    }
                    else
                    {
                        var argVal = args[argIndex + 1];
                        var convertedVal = Convert.ChangeType(argVal, p.PropInfo.PropertyType);
                        optVal.Add(p, convertedVal);
                        args.RemoveRange(argIndex, 2);
                    }
                }
            }

            // if help is requested there is no need to search for other arguments
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
                    optVal.Add(p, convertedVal);
                    args.RemoveRange(argIndex, 2);
                }
            }

            // if there are any arguments left, they are unknown, hence error
            if (args.Count > 0)
            {
                Valid = false;
                ErrorState = args.Aggregate("Unknown arguments:", (current, arg) => current += $" {arg}");
                return this;
            }

            // set all properties in Options object
            foreach (var (prop, val) in optVal)
            {
                prop.PropInfo.SetValue(opts, val);
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
            if (Valid) return (T)Options;
            return null;
        }

        #endregion

        #region Helpers

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
                var aliases = new List<string> { attr.Name.ToUpper(), attr.Alias.ToUpper() };
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

        #endregion
    }
}