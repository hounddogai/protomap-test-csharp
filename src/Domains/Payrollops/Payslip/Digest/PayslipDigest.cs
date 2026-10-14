namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip digest is in its life cycle.</summary>
public enum PayslipDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip digest. Its code is unique within a tenant.</summary>
public sealed class PayslipDigest(string code, string label, int updatedAt, double minimum, string salaryBand, string externalRef, string grade, bool approved, string ownerId, string employeeId, string costCenter, PayslipDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public string ExternalRef { get; set; } = externalRef;
    public string Grade { get; set; } = grade;
    public bool Approved { get; set; } = approved;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public string CostCenter { get; set; } = costCenter;
    public PayslipDigestStatus Status { get; set; } = status;

    /// <summary>Starts a payslip digest in status requested.</summary>
    public static PayslipDigest NewPayslipDigest(string code, string label)
    {
        return new PayslipDigest(code, label, 0, 0.0, "", "", "", false, "", "", "", PayslipDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipDigestStatus next)
    {
        switch (Status)
        {
            case PayslipDigestStatus.Requested:
                switch (next)
                {
                    case PayslipDigestStatus.Revoked:
                        return true;
                }
                return false;
            case PayslipDigestStatus.Granted:
                switch (next)
                {
                    case PayslipDigestStatus.Revoked:
                    case PayslipDigestStatus.Expired:
                        return true;
                }
                return false;
            case PayslipDigestStatus.Revoked:
                return next == PayslipDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Minimum + (double)UpdatedAt, 2);
    }

    public PayslipDigest Copy()
    {
        return new PayslipDigest(
            Code,
            Label,
            UpdatedAt,
            Minimum,
            SalaryBand,
            ExternalRef,
            Grade,
            Approved,
            OwnerId,
            EmployeeId,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
