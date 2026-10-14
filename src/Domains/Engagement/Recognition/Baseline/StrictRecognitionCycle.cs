namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict recognition cycle is in its life cycle.</summary>
public enum StrictRecognitionCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict recognition cycle as the recognition team keeps it.</summary>
public sealed class StrictRecognitionCycle(string code, string name, int durationMinutes, bool archived, double ratio, int limit, double budget, StrictRecognitionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public double Ratio { get; set; } = ratio;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public StrictRecognitionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a strict recognition cycle in status planned.</summary>
    public static StrictRecognitionCycle NewStrictRecognitionCycle(string code, string name)
    {
        return new StrictRecognitionCycle(code, name, 0, false, 0.0, 0, 0.0, StrictRecognitionCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRecognitionCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRecognitionCycleStatus next)
    {
        switch (Status)
        {
            case StrictRecognitionCycleStatus.Planned:
                switch (next)
                {
                    case StrictRecognitionCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictRecognitionCycleStatus.Scheduled:
                return next == StrictRecognitionCycleStatus.Completed;
            case StrictRecognitionCycleStatus.InProgress:
                switch (next)
                {
                    case StrictRecognitionCycleStatus.Completed:
                    case StrictRecognitionCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictRecognitionCycleStatus.Completed:
                switch (next)
                {
                    case StrictRecognitionCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictRecognitionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict recognition cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * Ratio, 2);
    }

    public StrictRecognitionCycle Copy()
    {
        return new StrictRecognitionCycle(Code, Name, DurationMinutes, Archived, Ratio, Limit, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
