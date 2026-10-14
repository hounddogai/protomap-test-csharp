namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall cycle is in its life cycle.</summary>
public enum TownHallCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A town hall cycle as the town hall team keeps it.</summary>
public sealed class TownHallCycle(string code, string label, double fte, double weight, int leaveDays, string unit, string externalRef, string region, bool pinned, TownHallCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public string Unit { get; set; } = unit;
    public string ExternalRef { get; set; } = externalRef;
    public string Region { get; set; } = region;
    public bool Pinned { get; set; } = pinned;
    public TownHallCycleStatus Status { get; set; } = status;

    /// <summary>Starts a town hall cycle in status proposed.</summary>
    public static TownHallCycle NewTownHallCycle(string code, string label)
    {
        return new TownHallCycle(code, label, 0.0, 0.0, 0, "", "", "", false, TownHallCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallCycleStatus next)
    {
        switch (Status)
        {
            case TownHallCycleStatus.Proposed:
                return next == TownHallCycleStatus.Approved || next == TownHallCycleStatus.Rejected;
            case TownHallCycleStatus.Approved:
                return next == TownHallCycleStatus.Rejected;
            case TownHallCycleStatus.Rejected:
                switch (next)
                {
                    case TownHallCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"town hall cycle"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays + Weight, 2);
    }

    public TownHallCycle Copy()
    {
        return new TownHallCycle(Code, Label, Fte, Weight, LeaveDays, Unit, ExternalRef, Region, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
