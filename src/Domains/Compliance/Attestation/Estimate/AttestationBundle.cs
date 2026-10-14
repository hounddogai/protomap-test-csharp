namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation bundle is in its life cycle.</summary>
public enum AttestationBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a attestation bundle and its current status.</summary>
public sealed class AttestationBundle(string code, string name, double maximum, string title, string unit, int endsAt, int capacity, int updatedAt, int durationMinutes, int expiresAt, int windowDays, AttestationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public AttestationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a attestation bundle in status requested.</summary>
    public static AttestationBundle NewAttestationBundle(string code, string name)
    {
        return new AttestationBundle(code, name, 0.0, "", "", 0, 0, 0, 0, 0, 0, AttestationBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationBundleStatus next)
    {
        switch (Status)
        {
            case AttestationBundleStatus.Requested:
                switch (next)
                {
                    case AttestationBundleStatus.Revoked:
                    case AttestationBundleStatus.Expired:
                        return true;
                }
                return false;
            case AttestationBundleStatus.Granted:
                switch (next)
                {
                    case AttestationBundleStatus.Revoked:
                        return true;
                }
                return false;
            case AttestationBundleStatus.Revoked:
                switch (next)
                {
                    case AttestationBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt - (double)DurationMinutes, 2);
    }

    public AttestationBundle Copy()
    {
        return new AttestationBundle(
            Code,
            Name,
            Maximum,
            Title,
            Unit,
            EndsAt,
            Capacity,
            UpdatedAt,
            DurationMinutes,
            ExpiresAt,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
