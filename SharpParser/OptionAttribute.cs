using System;
using System.ComponentModel.DataAnnotations;

namespace SharpParser
{
    public class Option : Attribute
    {
        private string _help;

        private bool _required;

        public Option(string alias, string name)
        {
            Alias = alias;
            Name = name;
            _help = string.Empty;
            _required = false;
        }


        public virtual string Alias { get; }

        public virtual string Name { get; }

        public virtual string Help
        {
            get => _help;
            set => _help = value;
        }

        public virtual bool Required
        {
            get => _required;
            set => _required = value;
        }
    }
}
