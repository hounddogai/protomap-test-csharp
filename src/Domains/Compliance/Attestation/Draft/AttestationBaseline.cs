namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation baseline is in its life cycle.</summary>
public enum AttestationBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A attestation baseline as the attestation team keeps it.</summary>
public sealed class AttestationBaseline(string code, string title, int sequence, string ownerId, int version, string name, string badge, int tenureMonths, string source, int batchSize, string reviewer, AttestationBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public AttestationBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a attestation baseline in status draft.</summary>
    public static AttestationBaseline NewAttestationBaseline(string code, string title)
    {
        return new AttestationBaseline(code, title, 0, "", 0, "", "", 0, "", 0, "", AttestationBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationBaselineStatus next)
    {
        switch (Status)
        {
            case AttestationBaselineStatus.Draft:
                switch (next)
                {
                    case AttestationBaselineStatus.Suspended:
                        return true;
                }
                return false;
            case AttestationBaselineStatus.Pending:
                return next == AttestationBaselineStatus.Suspended;
            case AttestationBaselineStatus.Active:
                switch (next)
                {
                    case AttestationBaselineStatus.Suspended:
                    case AttestationBaselineStatus.Closed:
                        return true;
                }
                return false;
            case AttestationBaselineStatus.Suspended:
                return next == AttestationBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AttestationBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)TenureMonths + (double)Version, 2);
    }

    public AttestationBaseline Copy()
    {
        return new AttestationBaseline(
            Code,
            Title,
            Sequence,
            OwnerId,
            Version,
            Name,
            Badge,
            TenureMonths,
            Source,
            BatchSize,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
