namespace Acme.Workforce.Compliance.Certification.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification manifest is in its life cycle.</summary>
public enum CertificationManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A certification manifest as the certification team keeps it.</summary>
public sealed class CertificationManifest(string code, string label, string externalRef, string name, int createdAt, int leaveDays, bool approved, string phone, bool visible, int sequence, string currency, CertificationManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public int Sequence { get; set; } = sequence;
    public string Currency { get; set; } = currency;
    public CertificationManifestStatus Status { get; set; } = status;

    /// <summary>Starts a certification manifest in status planned.</summary>
    public static CertificationManifest NewCertificationManifest(string code, string label)
    {
        return new CertificationManifest(
            code,
            label,
            "",
            "",
            0,
            0,
            false,
            "",
            false,
            0,
            "",
            CertificationManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationManifestStatus next)
    {
        switch (Status)
        {
            case CertificationManifestStatus.Planned:
                switch (next)
                {
                    case CertificationManifestStatus.InProgress:
                        return true;
                }
                return false;
            case CertificationManifestStatus.Scheduled:
                return next == CertificationManifestStatus.InProgress || next == CertificationManifestStatus.Completed;
            case CertificationManifestStatus.InProgress:
                switch (next)
                {
                    case CertificationManifestStatus.Completed:
                    case CertificationManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case CertificationManifestStatus.Completed:
                switch (next)
                {
                    case CertificationManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays - (double)Sequence, 2);
    }

    public CertificationManifest Copy()
    {
        return new CertificationManifest(
            Code,
            Label,
            ExternalRef,
            Name,
            CreatedAt,
            LeaveDays,
            Approved,
            Phone,
            Visible,
            Sequence,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
