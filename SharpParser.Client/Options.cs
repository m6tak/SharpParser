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

        [Option("i", "integer", Help = "Number argument", Required = true)]
        public int Number { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}\nNumber: {Number}\nVerbose: {Verbose}\nOtherSwitch: {OtherSwitch}";
        }
    }
}
