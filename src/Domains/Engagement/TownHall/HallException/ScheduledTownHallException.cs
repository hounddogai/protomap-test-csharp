namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled town hall exception is in its life cycle.</summary>
public enum ScheduledTownHallExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled town hall exception as the town hall team keeps it.</summary>
public sealed class ScheduledTownHallException(string code, string name, string jobCode, int revision, double rating, int attempts, string title, int version, ScheduledTownHallExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int Revision { get; set; } = revision;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public ScheduledTownHallExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled town hall exception in status proposed.</summary>
    public static ScheduledTownHallException NewScheduledTownHallException(string code, string name)
    {
        return new ScheduledTownHallException(
            code,
            name,
            "",
            0,
            0.0,
            0,
            "",
            0,
            ScheduledTownHallExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTownHallExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTownHallExceptionStatus next)
    {
        switch (Status)
        {
            case ScheduledTownHallExceptionStatus.Proposed:
                switch (next)
                {
                    case ScheduledTownHallExceptionStatus.Approved:
                    case ScheduledTownHallExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledTownHallExceptionStatus.Approved:
                switch (next)
                {
                    case ScheduledTownHallExceptionStatus.Rejected:
                        return true;
                }
                return false;
            case ScheduledTownHallExceptionStatus.Rejected:
                return next == ScheduledTownHallExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledTownHallExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled town hall exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scheduled town hall exception"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision - (double)Attempts, 2);
    }

    public ScheduledTownHallException Copy()
    {
        return new ScheduledTownHallException(
            Code,
            Name,
            JobCode,
            Revision,
            Rating,
            Attempts,
            Title,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
