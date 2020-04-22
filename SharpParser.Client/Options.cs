using System.Xml.Serialization;

namespace SharpParser.Client
{
    class Options
    {
        [Option("v", "verbose", Help = "Detailed program output")]
        public bool Verbose { get; set; }

        [Option("s", "switch", Help = "Some switch")]
        public bool OtherSwitch { get; set; }

        [Option("n", "name", Help = "Your name", Required = true)]
        public string Name { get; set; }
    }
}
