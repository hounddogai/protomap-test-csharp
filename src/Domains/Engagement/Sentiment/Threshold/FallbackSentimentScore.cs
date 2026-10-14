namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback sentiment score is in its life cycle.</summary>
public enum FallbackSentimentScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback sentiment score, with what the planners need to act on it.</summary>
public sealed class FallbackSentimentScore(string code, string label, Dictionary<string, string> attributes, string name, string country, string locale, string costCenter, int version, int leaveDays, int expiresAt, FallbackSentimentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public int Version { get; set; } = version;
    public int LeaveDays { get; set; } = leaveDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public FallbackSentimentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a fallback sentiment score in status draft.</summary>
    public static FallbackSentimentScore NewFallbackSentimentScore(string code, string label)
    {
        return new FallbackSentimentScore(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            0,
            0,
            0,
            FallbackSentimentScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSentimentScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSentimentScoreStatus next)
    {
        switch (Status)
        {
            case FallbackSentimentScoreStatus.Draft:
                switch (next)
                {
                    case FallbackSentimentScoreStatus.Active:
                        return true;
                }
                return false;
            case FallbackSentimentScoreStatus.Pending:
                return next == FallbackSentimentScoreStatus.Active || next == FallbackSentimentScoreStatus.Closed;
            case FallbackSentimentScoreStatus.Active:
                return next == FallbackSentimentScoreStatus.Suspended || next == FallbackSentimentScoreStatus.Closed;
            case FallbackSentimentScoreStatus.Suspended:
                switch (next)
                {
                    case FallbackSentimentScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSentimentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback sentiment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Version * (double)LeaveDays, 2);
    }

    public FallbackSentimentScore Copy()
    {
        return new FallbackSentimentScore(
            Code,
            Label,
            Attributes,
            Name,
            Country,
            Locale,
            CostCenter,
            Version,
            LeaveDays,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
