namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification exception is in its life cycle.</summary>
public enum CertificationExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a certification exception and its current status.</summary>
public sealed class CertificationException(string code, string label, double discount, string checksum, double margin, int tenureMonths, string grade, double score, int createdAt, int updatedAt, CertificationExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Checksum { get; set; } = checksum;
    public double Margin { get; set; } = margin;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public CertificationExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a certification exception in status created.</summary>
    public static CertificationException NewCertificationException(string code, string label)
    {
        return new CertificationException(
            code,
            label,
            0.0,
            "",
            0.0,
            0,
            "",
            0.0,
            0,
            0,
            CertificationExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationExceptionStatus next)
    {
        switch (Status)
        {
            case CertificationExceptionStatus.Created:
                return next == CertificationExceptionStatus.Published || next == CertificationExceptionStatus.Retired;
            case CertificationExceptionStatus.Validated:
                return next == CertificationExceptionStatus.Published;
            case CertificationExceptionStatus.Published:
                return next == CertificationExceptionStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CertificationExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Discount - Score, 2);
    }

    public CertificationException Copy()
    {
        return new CertificationException(
            Code,
            Label,
            Discount,
            Checksum,
            Margin,
            TenureMonths,
            Grade,
            Score,
            CreatedAt,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
