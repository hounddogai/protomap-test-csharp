namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered assignment agreement is in its life cycle.</summary>
public enum TieredAssignmentAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered assignment agreement. Its code is unique within a tenant.</summary>
public sealed class TieredAssignmentAgreement(string code, string label, bool visible, int createdAt, bool active, string summary, string salaryBand, TieredAssignmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int CreatedAt { get; set; } = createdAt;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public TieredAssignmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a tiered assignment agreement in status proposed.</summary>
    public static TieredAssignmentAgreement NewTieredAssignmentAgreement(string code, string label)
    {
        return new TieredAssignmentAgreement(
            code,
            label,
            false,
            0,
            false,
            "",
            "",
            TieredAssignmentAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAssignmentAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAssignmentAgreementStatus next)
    {
        switch (Status)
        {
            case TieredAssignmentAgreementStatus.Proposed:
                return next == TieredAssignmentAgreementStatus.Rejected;
            case TieredAssignmentAgreementStatus.Approved:
                return next == TieredAssignmentAgreementStatus.Rejected || next == TieredAssignmentAgreementStatus.Withdrawn;
            case TieredAssignmentAgreementStatus.Rejected:
                return next == TieredAssignmentAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredAssignmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered assignment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public TieredAssignmentAgreement Copy()
    {
        return new TieredAssignmentAgreement(Code, Label, Visible, CreatedAt, Active, Summary, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
