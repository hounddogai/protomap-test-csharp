namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall estimate is in its life cycle.</summary>
public enum TownHallEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a town hall estimate and its current status.</summary>
public sealed class TownHallEstimate(string code, string name, bool pinned, int leaveDays, bool active, int graceDays, TownHallEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public TownHallEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a town hall estimate in status planned.</summary>
    public static TownHallEstimate NewTownHallEstimate(string code, string name)
    {
        return new TownHallEstimate(code, name, false, 0, false, 0, TownHallEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallEstimateStatus next)
    {
        switch (Status)
        {
            case TownHallEstimateStatus.Planned:
                return next == TownHallEstimateStatus.Scheduled || next == TownHallEstimateStatus.Completed;
            case TownHallEstimateStatus.Scheduled:
                switch (next)
                {
                    case TownHallEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallEstimateStatus.InProgress:
                return next == TownHallEstimateStatus.Cancelled;
            case TownHallEstimateStatus.Completed:
                switch (next)
                {
                    case TownHallEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)GraceDays + (double)LeaveDays, 2);
    }

    public TownHallEstimate Copy()
    {
        return new TownHallEstimate(Code, Name, Pinned, LeaveDays, Active, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
