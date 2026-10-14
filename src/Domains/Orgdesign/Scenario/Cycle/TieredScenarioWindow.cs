namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered scenario window is in its life cycle.</summary>
public enum TieredScenarioWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered scenario window as the scenario team keeps it.</summary>
public sealed class TieredScenarioWindow(string code, string label, int windowDays, double maximum, string unit, double ratio, TieredScenarioWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public TieredScenarioWindowStatus Status { get; set; } = status;

    /// <summary>Starts a tiered scenario window in status proposed.</summary>
    public static TieredScenarioWindow NewTieredScenarioWindow(string code, string label)
    {
        return new TieredScenarioWindow(code, label, 0, 0.0, "", 0.0, TieredScenarioWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredScenarioWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredScenarioWindowStatus next)
    {
        switch (Status)
        {
            case TieredScenarioWindowStatus.Proposed:
                return next == TieredScenarioWindowStatus.Approved;
            case TieredScenarioWindowStatus.Approved:
                return next == TieredScenarioWindowStatus.Rejected || next == TieredScenarioWindowStatus.Withdrawn;
            case TieredScenarioWindowStatus.Rejected:
                switch (next)
                {
                    case TieredScenarioWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredScenarioWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered scenario window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)WindowDays - Ratio, 2);
    }

    public TieredScenarioWindow Copy()
    {
        return new TieredScenarioWindow(Code, Label, WindowDays, Maximum, Unit, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
