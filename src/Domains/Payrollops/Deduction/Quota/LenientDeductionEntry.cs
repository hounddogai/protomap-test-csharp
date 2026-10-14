namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction entry is in its life cycle.</summary>
public enum LenientDeductionEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient deduction entry. Its code is unique within a tenant.</summary>
public sealed class LenientDeductionEntry(string code, string title, string team, string ownerId, string phone, double ceiling, double fte, string employeeId, string department, string costCenter, LenientDeductionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public string CostCenter { get; set; } = costCenter;
    public LenientDeductionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction entry in status open.</summary>
    public static LenientDeductionEntry NewLenientDeductionEntry(string code, string title)
    {
        return new LenientDeductionEntry(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            LenientDeductionEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionEntryStatus next)
    {
        switch (Status)
        {
            case LenientDeductionEntryStatus.Open:
                return next == LenientDeductionEntryStatus.InReview;
            case LenientDeductionEntryStatus.InReview:
                switch (next)
                {
                    case LenientDeductionEntryStatus.Resolved:
                    case LenientDeductionEntryStatus.Reopened:
                        return true;
                }
                return false;
            case LenientDeductionEntryStatus.Resolved:
                switch (next)
                {
                    case LenientDeductionEntryStatus.Reopened:
                    case LenientDeductionEntryStatus.Archived:
                        return true;
                }
                return false;
            case LenientDeductionEntryStatus.Reopened:
                return next == LenientDeductionEntryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientDeductionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Fte * Ceiling, 2);
    }

    public LenientDeductionEntry Copy()
    {
        return new LenientDeductionEntry(
            Code,
            Title,
            Team,
            OwnerId,
            Phone,
            Ceiling,
            Fte,
            EmployeeId,
            Department,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
