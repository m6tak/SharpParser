# SharpParser
SharpParser is simple, idiomatic argument parser for .NET applications.

# Usage examples

### Arguments model
First you need to define model of your arguments. You can do this with **simple class** combined with **attribute** provided by SharpParser like so:  
```
class Options
{
    // Group attribute lets you group your arguments so user cannot use two arguments
    // from the same group. Default group is "none" and its ignored
    [Option("n", "name", Help = "Your name", Group = "name")]
    public string Name { get; set; }

    [Option("N", "nickname", Help = "Your nickname", Group = "name")]
    public string Nickname { get; set; }

    [Option("a", "age", Help = "Your age", Required = true)]
    public int Age { get; set; }

    // bool properties are switches, they should be optional and if user skips them
    // they are set to false
    [Option("g", "goodbye", Help = "Say goodbye")]
    public bool Goodbye { get; set; }

    // option with name "help" is special option reserved for manual purposes
    [Option("h", "help", Help = "Program manual")]
    public bool Help { get; set; }
}
```

### Parsing
Parser is very simple and intuitive
```
class Program
{
    // get help message generated for your arguments model
    private string _programManual = Parser.ConstructBasicManual<Options>();

    static void Main(string[] args)
    {
        // parse
        var options = new Parser(args) // create parser
            .ParseWith<Options>() // parse with your arguments model
            .OnError(ErrorHandler) // act on error
            .RetrieveOptions<Options>(); // get options, you get null if any error occurred

        // do stuff with parsed arguments...
        if(options.Help) {
            console.WriteLine(_programManual);
            return;
        }

        // optional arguments are set to null if they are not provided
        Console.WriteLine($"Hello {options.Name ?? options.Nickname ?? ("stranger")}.");

        // required arguments are always there in declared type
        Console.WriteLine($"You are { options.Age} years old");

        if(options.Goodbye) Console.WriteLine("Goodbye!");
    }

    // error handler
    static void ErrorHandler(string error)
    {
        Console.WriteLine($"Error! {error}");
        Console.WriteLine(_programManual);
        Console.ReadLine();
        Environment.Exit(0);
    }
}
```
And that's it. With SharpParser, handling user input gets really simple.  
Running this program like so: `dotnet app.dll -n Anon -a 21 -g` will give following output  
```
Hello Anon.
You are 21 years old
Goodbye
```

### Auto generated help message
With you model you can also specify help text which is than used to generate simple help message.
```
private string _programManual = Parser.ConstructBasicManual<Options>();

Console.WriteLine(_programManual);
```

Will generate following message for our options
```
Options:
--name -n                Your name
--nickname -N            Your nickname
*--age -a                Your age
--goodbye -g             Say goodbye
--help -h                Program manual
```
Required arguments are marked with `*`

# Present & Future

### Limitations
This is meant to be very simple, fast and lightweight parser. 
It only supports one value per argument, and may not be fully compatible with
every platform and .net sdk.

### Plans
I'm planning to ensure compatibility with all .net sdks and all platforms

# Get it
Nuget package *comming soon*