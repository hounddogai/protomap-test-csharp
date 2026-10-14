namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition claim policy.</summary>
public static class RecognitionClaimPolicy
{
    public static string RecognitionClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int RecognitionClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
