namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan allocation is in its life cycle.</summary>
public enum SuccessionPlanAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One succession plan allocation, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanAllocation(string code, string label, bool visible, int retries, string externalRef, int limit, bool approved, int startsAt, int windowDays, string skill, SuccessionPlanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int Retries { get; set; } = retries;
    public string ExternalRef { get; set; } = externalRef;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Skill { get; set; } = skill;
    public SuccessionPlanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan allocation in status open.</summary>
    public static SuccessionPlanAllocation NewSuccessionPlanAllocation(string code, string label)
    {
        return new SuccessionPlanAllocation(
            code,
            label,
            false,
            0,
            "",
            0,
            false,
            0,
            0,
            "",
            SuccessionPlanAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanAllocationStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanAllocationStatus.Open:
                return next == SuccessionPlanAllocationStatus.Resolved;
            case SuccessionPlanAllocationStatus.InReview:
                return next == SuccessionPlanAllocationStatus.Archived;
            case SuccessionPlanAllocationStatus.Resolved:
                return next == SuccessionPlanAllocationStatus.Reopened || next == SuccessionPlanAllocationStatus.Archived;
            case SuccessionPlanAllocationStatus.Reopened:
                return next == SuccessionPlanAllocationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays - (double)StartsAt, 2);
    }

    public SuccessionPlanAllocation Copy()
    {
        return new SuccessionPlanAllocation(
            Code,
            Label,
            Visible,
            Retries,
            ExternalRef,
            Limit,
            Approved,
            StartsAt,
            WindowDays,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
