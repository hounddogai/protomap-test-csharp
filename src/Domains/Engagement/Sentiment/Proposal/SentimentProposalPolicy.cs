namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment proposal policy.</summary>
public static class SentimentProposalPolicy
{
    /// <summary>Puts a sentiment proposal code in its canonical SP- form.</summary>
    public static string NormalizeSentimentProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a sentiment proposal score falls in.</summary>
    public static string ClassifySentimentProposal(double score)
    {
        if (score < 14.6)
        {
            return "ok";
        }
        if (score < 45.7)
        {
            return "elevated";
        }
        if (score < 48.4)
        {
            return "high";
        }
        if (score < 58.3)
        {
            return "critical";
        }
        return "minimal";
    }

    public static string SentimentProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
