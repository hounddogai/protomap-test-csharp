namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted town hall exception is in its life cycle.</summary>
public enum WeightedTownHallExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted town hall exception. Its code is unique within a tenant.</summary>
public sealed class WeightedTownHallException(string code, string name, int capacity, bool archived, int windowDays, double hours, string country, bool active, double score, string label, bool pinned, WeightedTownHallExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public double Hours { get; set; } = hours;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public WeightedTownHallExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a weighted town hall exception in status planned.</summary>
    public static WeightedTownHallException NewWeightedTownHallException(string code, string name)
    {
        return new WeightedTownHallException(
            code,
            name,
            0,
            false,
            0,
            0.0,
            "",
            false,
            0.0,
            "",
            false,
            WeightedTownHallExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTownHallExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTownHallExceptionStatus next)
    {
        switch (Status)
        {
            case WeightedTownHallExceptionStatus.Planned:
                return next == WeightedTownHallExceptionStatus.Completed;
            case WeightedTownHallExceptionStatus.Scheduled:
                return next == WeightedTownHallExceptionStatus.Completed || next == WeightedTownHallExceptionStatus.Cancelled;
            case WeightedTownHallExceptionStatus.InProgress:
                return next == WeightedTownHallExceptionStatus.Cancelled;
            case WeightedTownHallExceptionStatus.Completed:
                return next == WeightedTownHallExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedTownHallExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted town hall exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Hours - Score, 2);
    }

    public WeightedTownHallException Copy()
    {
        return new WeightedTownHallException(
            Code,
            Name,
            Capacity,
            Archived,
            WindowDays,
            Hours,
            Country,
            Active,
            Score,
            Label,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
