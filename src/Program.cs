public class Program
{

  private static readonly string? FUNCTION_TOKEN = Environment.GetEnvironmentVariable("AZ_FUNCTION_TOKEN");
  public static readonly string FUNCTION_URL = $"https://sas.docvault.shonx.net/api/GetSASURL?code={FUNCTION_TOKEN}";

  public static void Main(string[] args)
  {
    if (String.IsNullOrEmpty(FUNCTION_TOKEN))
    {
      throw new InvalidOperationException("FUNCTION_TOKEN not present.");
    }
    new WebHandler(args);
  }
}

