namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending sentiment variance is in its life cycle.</summary>
public enum PendingSentimentVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending sentiment variance, with what the planners need to act on it.</summary>
public sealed class PendingSentimentVariance(string code, string name, double floorValue, bool visible, int leaveDays, PendingSentimentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public PendingSentimentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pending sentiment variance in status new.</summary>
    public static PendingSentimentVariance NewPendingSentimentVariance(string code, string name)
    {
        return new PendingSentimentVariance(code, name, 0.0, false, 0, PendingSentimentVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSentimentVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSentimentVarianceStatus next)
    {
        switch (Status)
        {
            case PendingSentimentVarianceStatus.New:
                switch (next)
                {
                    case PendingSentimentVarianceStatus.Done:
                    case PendingSentimentVarianceStatus.Failed:
                        return true;
                }
                return false;
            case PendingSentimentVarianceStatus.Queued:
                switch (next)
                {
                    case PendingSentimentVarianceStatus.Done:
                        return true;
                }
                return false;
            case PendingSentimentVarianceStatus.Running:
                return next == PendingSentimentVarianceStatus.Done || next == PendingSentimentVarianceStatus.Failed;
            case PendingSentimentVarianceStatus.Done:
                return next == PendingSentimentVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingSentimentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending sentiment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)LeaveDays + FloorValue, 2);
    }

    public PendingSentimentVariance Copy()
    {
        return new PendingSentimentVariance(Code, Name, FloorValue, Visible, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
