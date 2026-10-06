using AIDBAgentBusiness;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        string apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            ?? throw new InvalidOperationException("Set the DEEPSEEK_API_KEY environment variable.");
        AIBE aibe = new AIBE("https://api.deepseek.com" , apiKey , "deepseek-v4-flash");

        Console.Write("Enter your prompt: ");
        while (Console.ReadLine() is string prompt && !string.IsNullOrWhiteSpace(prompt))
        {
            try
            {
                var answer = await aibe.SendPromptAsync(prompt);//"Write a C# method to read a JSON file."

                Console.WriteLine("AI says: " + answer);


                Console.Write("Enter your prompt: ");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
            //var builder = WebApplication.CreateBuilder(args);
            //var app = builder.Build();

            //app.MapGet("/" , () => "Hello World!");

            //app.Run();
        }
    }
