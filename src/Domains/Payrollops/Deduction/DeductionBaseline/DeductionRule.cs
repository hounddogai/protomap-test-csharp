namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction rule is in its life cycle.</summary>
public enum DeductionRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A deduction rule. Its code is unique within a tenant.</summary>
public sealed class DeductionRule(string code, string name, int limit, string checksum, int durationMinutes, string managerId, string team, int version, string unit, DeductionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public int Version { get; set; } = version;
    public string Unit { get; set; } = unit;
    public DeductionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a deduction rule in status idle.</summary>
    public static DeductionRule NewDeductionRule(string code, string name)
    {
        return new DeductionRule(code, name, 0, "", 0, "", "", 0, "", DeductionRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionRuleStatus next)
    {
        switch (Status)
        {
            case DeductionRuleStatus.Idle:
                return next == DeductionRuleStatus.Warming;
            case DeductionRuleStatus.Warming:
                return next == DeductionRuleStatus.Ready || next == DeductionRuleStatus.Stopped;
            case DeductionRuleStatus.Ready:
                switch (next)
                {
                    case DeductionRuleStatus.Stopped:
                        return true;
                }
                return false;
            case DeductionRuleStatus.Draining:
                return next == DeductionRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeductionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"deduction rule"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version * (double)DurationMinutes, 2);
    }

    public DeductionRule Copy()
    {
        return new DeductionRule(
            Code,
            Name,
            Limit,
            Checksum,
            DurationMinutes,
            ManagerId,
            Team,
            Version,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
