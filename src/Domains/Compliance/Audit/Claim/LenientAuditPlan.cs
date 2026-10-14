namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient audit plan is in its life cycle.</summary>
public enum LenientAuditPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient audit plan. Its code is unique within a tenant.</summary>
public sealed class LenientAuditPlan(string code, string label, string timezone, int createdAt, string costCenter, int endsAt, string externalRef, LenientAuditPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public LenientAuditPlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient audit plan in status proposed.</summary>
    public static LenientAuditPlan NewLenientAuditPlan(string code, string label)
    {
        return new LenientAuditPlan(code, label, "", 0, "", 0, "", LenientAuditPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAuditPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAuditPlanStatus next)
    {
        switch (Status)
        {
            case LenientAuditPlanStatus.Proposed:
                return next == LenientAuditPlanStatus.Approved || next == LenientAuditPlanStatus.Rejected;
            case LenientAuditPlanStatus.Approved:
                return next == LenientAuditPlanStatus.Rejected || next == LenientAuditPlanStatus.Withdrawn;
            case LenientAuditPlanStatus.Rejected:
                return next == LenientAuditPlanStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientAuditPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient audit plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)EndsAt + (double)CreatedAt, 2);
    }

    public LenientAuditPlan Copy()
    {
        return new LenientAuditPlan(Code, Label, Timezone, CreatedAt, CostCenter, EndsAt, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
