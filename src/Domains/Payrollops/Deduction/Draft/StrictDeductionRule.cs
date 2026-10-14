namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict deduction rule is in its life cycle.</summary>
public enum StrictDeductionRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict deduction rule. Its code is unique within a tenant.</summary>
public sealed class StrictDeductionRule(string code, string label, string managerId, bool approved, string skill, int durationMinutes, StrictDeductionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public StrictDeductionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a strict deduction rule in status requested.</summary>
    public static StrictDeductionRule NewStrictDeductionRule(string code, string label)
    {
        return new StrictDeductionRule(code, label, "", false, "", 0, StrictDeductionRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeductionRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeductionRuleStatus next)
    {
        switch (Status)
        {
            case StrictDeductionRuleStatus.Requested:
                return next == StrictDeductionRuleStatus.Granted || next == StrictDeductionRuleStatus.Expired;
            case StrictDeductionRuleStatus.Granted:
                return next == StrictDeductionRuleStatus.Revoked;
            case StrictDeductionRuleStatus.Revoked:
                return next == StrictDeductionRuleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictDeductionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict deduction rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public StrictDeductionRule Copy()
    {
        return new StrictDeductionRule(Code, Label, ManagerId, Approved, Skill, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
