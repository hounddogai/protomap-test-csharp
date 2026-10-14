namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered sentiment adjustment is in its life cycle.</summary>
public enum TieredSentimentAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a tiered sentiment adjustment and its current status.</summary>
public sealed class TieredSentimentAdjustment(string code, string label, int retries, int createdAt, double score, TieredSentimentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public double Score { get; set; } = score;
    public TieredSentimentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered sentiment adjustment in status requested.</summary>
    public static TieredSentimentAdjustment NewTieredSentimentAdjustment(string code, string label)
    {
        return new TieredSentimentAdjustment(code, label, 0, 0, 0.0, TieredSentimentAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSentimentAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSentimentAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredSentimentAdjustmentStatus.Requested:
                return next == TieredSentimentAdjustmentStatus.Revoked;
            case TieredSentimentAdjustmentStatus.Granted:
                return next == TieredSentimentAdjustmentStatus.Expired;
            case TieredSentimentAdjustmentStatus.Revoked:
                return next == TieredSentimentAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredSentimentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered sentiment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Score + (double)Retries, 2);
    }

    public TieredSentimentAdjustment Copy()
    {
        return new TieredSentimentAdjustment(Code, Label, Retries, CreatedAt, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
