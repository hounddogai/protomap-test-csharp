namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification checkpoint is in its life cycle.</summary>
public enum CertificationCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A certification checkpoint as the certification team keeps it.</summary>
public sealed class CertificationCheckpoint(string code, string label, int revision, string grade, string currency, int createdAt, string externalRef, double minimum, string phone, CertificationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public CertificationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a certification checkpoint in status draft.</summary>
    public static CertificationCheckpoint NewCertificationCheckpoint(string code, string label)
    {
        return new CertificationCheckpoint(
            code,
            label,
            0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            CertificationCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationCheckpointStatus next)
    {
        switch (Status)
        {
            case CertificationCheckpointStatus.Draft:
                return next == CertificationCheckpointStatus.Active || next == CertificationCheckpointStatus.Suspended;
            case CertificationCheckpointStatus.Pending:
                switch (next)
                {
                    case CertificationCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case CertificationCheckpointStatus.Active:
                return next == CertificationCheckpointStatus.Suspended;
            case CertificationCheckpointStatus.Suspended:
                switch (next)
                {
                    case CertificationCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Minimum + (double)CreatedAt, 2);
    }

    public CertificationCheckpoint Copy()
    {
        return new CertificationCheckpoint(
            Code,
            Label,
            Revision,
            Grade,
            Currency,
            CreatedAt,
            ExternalRef,
            Minimum,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
