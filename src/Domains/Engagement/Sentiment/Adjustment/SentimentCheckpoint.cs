namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment checkpoint is in its life cycle.</summary>
public enum SentimentCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A sentiment checkpoint. Its code is unique within a tenant.</summary>
public sealed class SentimentCheckpoint(string code, string name, string unit, int graceDays, string phone, double fte, SentimentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int GraceDays { get; set; } = graceDays;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public SentimentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment checkpoint in status planned.</summary>
    public static SentimentCheckpoint NewSentimentCheckpoint(string code, string name)
    {
        return new SentimentCheckpoint(code, name, "", 0, "", 0.0, SentimentCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentCheckpointStatus next)
    {
        switch (Status)
        {
            case SentimentCheckpointStatus.Planned:
                return next == SentimentCheckpointStatus.InProgress || next == SentimentCheckpointStatus.Completed;
            case SentimentCheckpointStatus.Scheduled:
                return next == SentimentCheckpointStatus.Completed;
            case SentimentCheckpointStatus.InProgress:
                switch (next)
                {
                    case SentimentCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case SentimentCheckpointStatus.Completed:
                return next == SentimentCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SentimentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Fte * (double)GraceDays, 2);
    }

    public SentimentCheckpoint Copy()
    {
        return new SentimentCheckpoint(Code, Name, Unit, GraceDays, Phone, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
