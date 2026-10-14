namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled reorg exception is in its life cycle.</summary>
public enum ScheduledReorgExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scheduled reorg exception and its current status.</summary>
public sealed class ScheduledReorgException(string code, string name, string region, int attempts, int limit, int windowDays, ScheduledReorgExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int Attempts { get; set; } = attempts;
    public int Limit { get; set; } = limit;
    public int WindowDays { get; set; } = windowDays;
    public ScheduledReorgExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled reorg exception in status planned.</summary>
    public static ScheduledReorgException NewScheduledReorgException(string code, string name)
    {
        return new ScheduledReorgException(code, name, "", 0, 0, 0, ScheduledReorgExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledReorgExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledReorgExceptionStatus next)
    {
        switch (Status)
        {
            case ScheduledReorgExceptionStatus.Planned:
                return next == ScheduledReorgExceptionStatus.Completed;
            case ScheduledReorgExceptionStatus.Scheduled:
                switch (next)
                {
                    case ScheduledReorgExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case ScheduledReorgExceptionStatus.InProgress:
                switch (next)
                {
                    case ScheduledReorgExceptionStatus.Completed:
                    case ScheduledReorgExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledReorgExceptionStatus.Completed:
                return next == ScheduledReorgExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledReorgExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled reorg exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled reorg exception"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Attempts - (double)WindowDays, 2);
    }

    public ScheduledReorgException Copy()
    {
        return new ScheduledReorgException(Code, Name, Region, Attempts, Limit, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
