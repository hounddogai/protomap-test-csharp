namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered sentiment target is in its life cycle.</summary>
public enum TieredSentimentTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered sentiment target as the sentiment team keeps it.</summary>
public sealed class TieredSentimentTarget(string code, string label, int expiresAt, double cost, int capacity, TieredSentimentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public TieredSentimentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered sentiment target in status requested.</summary>
    public static TieredSentimentTarget NewTieredSentimentTarget(string code, string label)
    {
        return new TieredSentimentTarget(code, label, 0, 0.0, 0, TieredSentimentTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSentimentTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSentimentTargetStatus next)
    {
        switch (Status)
        {
            case TieredSentimentTargetStatus.Requested:
                return next == TieredSentimentTargetStatus.Granted;
            case TieredSentimentTargetStatus.Granted:
                switch (next)
                {
                    case TieredSentimentTargetStatus.Revoked:
                    case TieredSentimentTargetStatus.Expired:
                        return true;
                }
                return false;
            case TieredSentimentTargetStatus.Revoked:
                return next == TieredSentimentTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredSentimentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered sentiment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered sentiment target"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Capacity * Cost, 2);
    }

    public TieredSentimentTarget Copy()
    {
        return new TieredSentimentTarget(Code, Label, ExpiresAt, Cost, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
