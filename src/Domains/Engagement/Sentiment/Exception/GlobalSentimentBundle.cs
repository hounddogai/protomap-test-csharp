namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global sentiment bundle is in its life cycle.</summary>
public enum GlobalSentimentBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a global sentiment bundle and its current status.</summary>
public sealed class GlobalSentimentBundle(string code, string name, string segment, string timezone, double weight, int endsAt, double score, double cost, GlobalSentimentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public int EndsAt { get; set; } = endsAt;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public GlobalSentimentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global sentiment bundle in status draft.</summary>
    public static GlobalSentimentBundle NewGlobalSentimentBundle(string code, string name)
    {
        return new GlobalSentimentBundle(code, name, "", "", 0.0, 0, 0.0, 0.0, GlobalSentimentBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSentimentBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSentimentBundleStatus next)
    {
        switch (Status)
        {
            case GlobalSentimentBundleStatus.Draft:
                return next == GlobalSentimentBundleStatus.Pending || next == GlobalSentimentBundleStatus.Active;
            case GlobalSentimentBundleStatus.Pending:
                return next == GlobalSentimentBundleStatus.Closed;
            case GlobalSentimentBundleStatus.Active:
                switch (next)
                {
                    case GlobalSentimentBundleStatus.Suspended:
                    case GlobalSentimentBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalSentimentBundleStatus.Suspended:
                switch (next)
                {
                    case GlobalSentimentBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalSentimentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global sentiment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global sentiment bundle"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Score * (double)EndsAt, 2);
    }

    public GlobalSentimentBundle Copy()
    {
        return new GlobalSentimentBundle(Code, Name, Segment, Timezone, Weight, EndsAt, Score, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
