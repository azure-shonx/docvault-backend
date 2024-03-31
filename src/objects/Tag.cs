using Newtonsoft.Json;

public class Tag
{
    public string Key { get; }
    public string Value { get; }

    public Tag(string Key, string Value)
    {
        this.Key = Key;
        this.Value = Value;
    }


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}