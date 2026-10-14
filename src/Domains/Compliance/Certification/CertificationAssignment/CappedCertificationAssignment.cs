namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped certification assignment is in its life cycle.</summary>
public enum CappedCertificationAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped certification assignment, with what the planners need to act on it.</summary>
public sealed class CappedCertificationAssignment(string code, string title, int retries, int attempts, double ratio, int priority, string salaryBand, double discount, int expiresAt, string locale, string unit, CappedCertificationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public double Ratio { get; set; } = ratio;
    public int Priority { get; set; } = priority;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Locale { get; set; } = locale;
    public string Unit { get; set; } = unit;
    public CappedCertificationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped certification assignment in status created.</summary>
    public static CappedCertificationAssignment NewCappedCertificationAssignment(string code, string title)
    {
        return new CappedCertificationAssignment(
            code,
            title,
            0,
            0,
            0.0,
            0,
            "",
            0.0,
            0,
            "",
            "",
            CappedCertificationAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCertificationAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCertificationAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedCertificationAssignmentStatus.Created:
                return next == CappedCertificationAssignmentStatus.Published || next == CappedCertificationAssignmentStatus.Retired;
            case CappedCertificationAssignmentStatus.Validated:
                switch (next)
                {
                    case CappedCertificationAssignmentStatus.Published:
                    case CappedCertificationAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case CappedCertificationAssignmentStatus.Published:
                switch (next)
                {
                    case CappedCertificationAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCertificationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped certification assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - (double)ExpiresAt, 2);
    }

    public CappedCertificationAssignment Copy()
    {
        return new CappedCertificationAssignment(
            Code,
            Title,
            Retries,
            Attempts,
            Ratio,
            Priority,
            SalaryBand,
            Discount,
            ExpiresAt,
            Locale,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
