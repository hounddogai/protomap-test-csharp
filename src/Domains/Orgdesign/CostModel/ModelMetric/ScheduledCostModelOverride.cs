namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled cost model override is in its life cycle.</summary>
public enum ScheduledCostModelOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled cost model override as the cost model team keeps it.</summary>
public sealed class ScheduledCostModelOverride(string code, string name, double rate, double budget, string summary, int sequence, bool pinned, int leaveDays, int version, double hours, ScheduledCostModelOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public int Sequence { get; set; } = sequence;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public ScheduledCostModelOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled cost model override in status proposed.</summary>
    public static ScheduledCostModelOverride NewScheduledCostModelOverride(string code, string name)
    {
        return new ScheduledCostModelOverride(
            code,
            name,
            0.0,
            0.0,
            "",
            0,
            false,
            0,
            0,
            0.0,
            ScheduledCostModelOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledCostModelOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledCostModelOverrideStatus next)
    {
        switch (Status)
        {
            case ScheduledCostModelOverrideStatus.Proposed:
                return next == ScheduledCostModelOverrideStatus.Withdrawn;
            case ScheduledCostModelOverrideStatus.Approved:
                return next == ScheduledCostModelOverrideStatus.Withdrawn;
            case ScheduledCostModelOverrideStatus.Rejected:
                return next == ScheduledCostModelOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledCostModelOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled cost model override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Version - (double)LeaveDays, 2);
    }

    public ScheduledCostModelOverride Copy()
    {
        return new ScheduledCostModelOverride(
            Code,
            Name,
            Rate,
            Budget,
            Summary,
            Sequence,
            Pinned,
            LeaveDays,
            Version,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
