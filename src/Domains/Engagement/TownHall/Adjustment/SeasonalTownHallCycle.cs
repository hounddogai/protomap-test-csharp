namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal town hall cycle is in its life cycle.</summary>
public enum SeasonalTownHallCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal town hall cycle as the town hall team keeps it.</summary>
public sealed class SeasonalTownHallCycle(string code, string label, int limit, string country, int windowDays, int graceDays, SeasonalTownHallCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Country { get; set; } = country;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public SeasonalTownHallCycleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal town hall cycle in status requested.</summary>
    public static SeasonalTownHallCycle NewSeasonalTownHallCycle(string code, string label)
    {
        return new SeasonalTownHallCycle(code, label, 0, "", 0, 0, SeasonalTownHallCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTownHallCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTownHallCycleStatus next)
    {
        switch (Status)
        {
            case SeasonalTownHallCycleStatus.Requested:
                return next == SeasonalTownHallCycleStatus.Granted;
            case SeasonalTownHallCycleStatus.Granted:
                return next == SeasonalTownHallCycleStatus.Revoked || next == SeasonalTownHallCycleStatus.Expired;
            case SeasonalTownHallCycleStatus.Revoked:
                return next == SeasonalTownHallCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalTownHallCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal town hall cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Limit * (double)WindowDays, 2);
    }

    public SeasonalTownHallCycle Copy()
    {
        return new SeasonalTownHallCycle(Code, Label, Limit, Country, WindowDays, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
