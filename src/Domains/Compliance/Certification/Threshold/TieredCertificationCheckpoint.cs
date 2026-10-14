namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered certification checkpoint is in its life cycle.</summary>
public enum TieredCertificationCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered certification checkpoint as the certification team keeps it.</summary>
public sealed class TieredCertificationCheckpoint(string code, string name, string jobCode, int updatedAt, string currency, int version, TieredCertificationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public TieredCertificationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a tiered certification checkpoint in status proposed.</summary>
    public static TieredCertificationCheckpoint NewTieredCertificationCheckpoint(string code, string name)
    {
        return new TieredCertificationCheckpoint(
            code,
            name,
            "",
            0,
            "",
            0,
            TieredCertificationCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCertificationCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCertificationCheckpointStatus next)
    {
        switch (Status)
        {
            case TieredCertificationCheckpointStatus.Proposed:
                switch (next)
                {
                    case TieredCertificationCheckpointStatus.Approved:
                    case TieredCertificationCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredCertificationCheckpointStatus.Approved:
                switch (next)
                {
                    case TieredCertificationCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case TieredCertificationCheckpointStatus.Rejected:
                return next == TieredCertificationCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredCertificationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered certification checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Version - (double)UpdatedAt, 2);
    }

    public TieredCertificationCheckpoint Copy()
    {
        return new TieredCertificationCheckpoint(Code, Name, JobCode, UpdatedAt, Currency, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
