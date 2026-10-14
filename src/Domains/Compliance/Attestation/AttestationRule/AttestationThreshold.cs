namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation threshold is in its life cycle.</summary>
public enum AttestationThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A attestation threshold as the attestation team keeps it.</summary>
public sealed class AttestationThreshold(string code, string name, string externalRef, double budget, int createdAt, int updatedAt, string channel, AttestationThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Budget { get; set; } = budget;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public AttestationThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a attestation threshold in status draft.</summary>
    public static AttestationThreshold NewAttestationThreshold(string code, string name)
    {
        return new AttestationThreshold(code, name, "", 0.0, 0, 0, "", AttestationThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationThresholdStatus next)
    {
        switch (Status)
        {
            case AttestationThresholdStatus.Draft:
                switch (next)
                {
                    case AttestationThresholdStatus.Closed:
                        return true;
                }
                return false;
            case AttestationThresholdStatus.Pending:
                switch (next)
                {
                    case AttestationThresholdStatus.Closed:
                        return true;
                }
                return false;
            case AttestationThresholdStatus.Active:
                switch (next)
                {
                    case AttestationThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case AttestationThresholdStatus.Suspended:
                return next == AttestationThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AttestationThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"attestation threshold"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt + (double)UpdatedAt, 2);
    }

    public AttestationThreshold Copy()
    {
        return new AttestationThreshold(Code, Name, ExternalRef, Budget, CreatedAt, UpdatedAt, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
