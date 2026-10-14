namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip quota is in its life cycle.</summary>
public enum PayslipQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A payslip quota as the payslip team keeps it.</summary>
public sealed class PayslipQuota(string code, string title, bool approved, bool archived, string checksum, double maximum, int expiresAt, string grade, string jobCode, PayslipQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public bool Archived { get; set; } = archived;
    public string Checksum { get; set; } = checksum;
    public double Maximum { get; set; } = maximum;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public string JobCode { get; set; } = jobCode;
    public PayslipQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a payslip quota in status created.</summary>
    public static PayslipQuota NewPayslipQuota(string code, string title)
    {
        return new PayslipQuota(code, title, false, false, "", 0.0, 0, "", "", PayslipQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipQuotaStatus next)
    {
        switch (Status)
        {
            case PayslipQuotaStatus.Created:
                return next == PayslipQuotaStatus.Published;
            case PayslipQuotaStatus.Validated:
                switch (next)
                {
                    case PayslipQuotaStatus.Published:
                        return true;
                }
                return false;
            case PayslipQuotaStatus.Published:
                return next == PayslipQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayslipQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)ExpiresAt - Maximum, 2);
    }

    public PayslipQuota Copy()
    {
        return new PayslipQuota(
            Code,
            Title,
            Approved,
            Archived,
            Checksum,
            Maximum,
            ExpiresAt,
            Grade,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
