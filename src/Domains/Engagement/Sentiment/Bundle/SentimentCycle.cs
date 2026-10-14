namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment cycle is in its life cycle.</summary>
public enum SentimentCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a sentiment cycle and its current status.</summary>
public sealed class SentimentCycle(string code, string name, string reviewer, string notes, int priority, double fte, double score, double amount, SentimentCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public SentimentCycleStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment cycle in status open.</summary>
    public static SentimentCycle NewSentimentCycle(string code, string name)
    {
        return new SentimentCycle(code, name, "", "", 0, 0.0, 0.0, 0.0, SentimentCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentCycleStatus next)
    {
        switch (Status)
        {
            case SentimentCycleStatus.Open:
                switch (next)
                {
                    case SentimentCycleStatus.Reopened:
                        return true;
                }
                return false;
            case SentimentCycleStatus.InReview:
                return next == SentimentCycleStatus.Resolved || next == SentimentCycleStatus.Archived;
            case SentimentCycleStatus.Resolved:
                return next == SentimentCycleStatus.Reopened || next == SentimentCycleStatus.Archived;
            case SentimentCycleStatus.Reopened:
                return next == SentimentCycleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SentimentCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment cycle"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Amount - Fte, 2);
    }

    public SentimentCycle Copy()
    {
        return new SentimentCycle(Code, Name, Reviewer, Notes, Priority, Fte, Score, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
