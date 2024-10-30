public class NobleResponses
{
    public static readonly string[] rejections = new string[]
    {
        "...",
        "Is this some strange joke?",
        "Who put you up to this?",
    };

    public static string GetRandomRejection()
    {
        var index = UnityEngine.Random.Range(0, rejections.Length);
        return rejections[index];
    }
}
