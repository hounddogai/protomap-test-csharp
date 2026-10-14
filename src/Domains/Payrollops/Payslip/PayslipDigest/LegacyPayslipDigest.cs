namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy payslip digest is in its life cycle.</summary>
public enum LegacyPayslipDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy payslip digest. Its code is unique within a tenant.</summary>
public sealed class LegacyPayslipDigest(string code, string title, string reviewer, string ownerId, string managerId, int durationMinutes, int graceDays, LegacyPayslipDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string OwnerId { get; set; } = ownerId;
    public string ManagerId { get; set; } = managerId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public LegacyPayslipDigestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy payslip digest in status proposed.</summary>
    public static LegacyPayslipDigest NewLegacyPayslipDigest(string code, string title)
    {
        return new LegacyPayslipDigest(code, title, "", "", "", 0, 0, LegacyPayslipDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayslipDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayslipDigestStatus next)
    {
        switch (Status)
        {
            case LegacyPayslipDigestStatus.Proposed:
                switch (next)
                {
                    case LegacyPayslipDigestStatus.Approved:
                        return true;
                }
                return false;
            case LegacyPayslipDigestStatus.Approved:
                return next == LegacyPayslipDigestStatus.Rejected;
            case LegacyPayslipDigestStatus.Rejected:
                switch (next)
                {
                    case LegacyPayslipDigestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayslipDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy payslip digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)DurationMinutes + (double)GraceDays, 2);
    }

    public LegacyPayslipDigest Copy()
    {
        return new LegacyPayslipDigest(
            Code,
            Title,
            Reviewer,
            OwnerId,
            ManagerId,
            DurationMinutes,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
