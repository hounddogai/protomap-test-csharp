namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional merit cycle slot is in its life cycle.</summary>
public enum RegionalMeritCycleSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional merit cycle slot as the merit cycle team keeps it.</summary>
public sealed class RegionalMeritCycleSlot(string code, string label, string name, string salaryBand, double amount, RegionalMeritCycleSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Amount { get; set; } = amount;
    public RegionalMeritCycleSlotStatus Status { get; set; } = status;

    /// <summary>Starts a regional merit cycle slot in status idle.</summary>
    public static RegionalMeritCycleSlot NewRegionalMeritCycleSlot(string code, string label)
    {
        return new RegionalMeritCycleSlot(code, label, "", "", 0.0, RegionalMeritCycleSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalMeritCycleSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalMeritCycleSlotStatus next)
    {
        switch (Status)
        {
            case RegionalMeritCycleSlotStatus.Idle:
                return next == RegionalMeritCycleSlotStatus.Warming || next == RegionalMeritCycleSlotStatus.Stopped;
            case RegionalMeritCycleSlotStatus.Warming:
                return next == RegionalMeritCycleSlotStatus.Ready || next == RegionalMeritCycleSlotStatus.Draining;
            case RegionalMeritCycleSlotStatus.Ready:
                return next == RegionalMeritCycleSlotStatus.Draining;
            case RegionalMeritCycleSlotStatus.Draining:
                return next == RegionalMeritCycleSlotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalMeritCycleSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional merit cycle slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional merit cycle slot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RegionalMeritCycleSlot Copy()
    {
        return new RegionalMeritCycleSlot(Code, Label, Name, SalaryBand, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
