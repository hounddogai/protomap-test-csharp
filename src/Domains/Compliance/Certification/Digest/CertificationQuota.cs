namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification quota is in its life cycle.</summary>
public enum CertificationQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A certification quota as the certification team keeps it.</summary>
public sealed class CertificationQuota(string code, string title, int updatedAt, string externalRef, string notes, double threshold, double budget, string employeeId, int sequence, CertificationQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public string EmployeeId { get; set; } = employeeId;
    public int Sequence { get; set; } = sequence;
    public CertificationQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a certification quota in status created.</summary>
    public static CertificationQuota NewCertificationQuota(string code, string title)
    {
        return new CertificationQuota(code, title, 0, "", "", 0.0, 0.0, "", 0, CertificationQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationQuotaStatus next)
    {
        switch (Status)
        {
            case CertificationQuotaStatus.Created:
                switch (next)
                {
                    case CertificationQuotaStatus.Validated:
                        return true;
                }
                return false;
            case CertificationQuotaStatus.Validated:
                return next == CertificationQuotaStatus.Retired;
            case CertificationQuotaStatus.Published:
                switch (next)
                {
                    case CertificationQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold + (double)UpdatedAt, 2);
    }

    public CertificationQuota Copy()
    {
        return new CertificationQuota(
            Code,
            Title,
            UpdatedAt,
            ExternalRef,
            Notes,
            Threshold,
            Budget,
            EmployeeId,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
