namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of sentiment reservation policy.</summary>
public static class SentimentReservationPolicy
{
    public static List<string> MergeSentimentReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a sentiment reservation code in its canonical SR- form.</summary>
    public static string NormalizeSentimentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
