namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy baseline is in its life cycle.</summary>
public enum PolicyBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One policy baseline, with what the planners need to act on it.</summary>
public sealed class PolicyBaseline(string code, string name, int capacity, double maximum, string label, double floorValue, int durationMinutes, double amount, double ratio, double fte, PolicyBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Maximum { get; set; } = maximum;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public PolicyBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a policy baseline in status planned.</summary>
    public static PolicyBaseline NewPolicyBaseline(string code, string name)
    {
        return new PolicyBaseline(code, name, 0, 0.0, "", 0.0, 0, 0.0, 0.0, 0.0, PolicyBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyBaselineStatus next)
    {
        switch (Status)
        {
            case PolicyBaselineStatus.Planned:
                return next == PolicyBaselineStatus.Completed;
            case PolicyBaselineStatus.Scheduled:
                switch (next)
                {
                    case PolicyBaselineStatus.Completed:
                        return true;
                }
                return false;
            case PolicyBaselineStatus.InProgress:
                return next == PolicyBaselineStatus.Completed || next == PolicyBaselineStatus.Cancelled;
            case PolicyBaselineStatus.Completed:
                return next == PolicyBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue - Maximum, 2);
    }

    public PolicyBaseline Copy()
    {
        return new PolicyBaseline(
            Code,
            Name,
            Capacity,
            Maximum,
            Label,
            FloorValue,
            DurationMinutes,
            Amount,
            Ratio,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
