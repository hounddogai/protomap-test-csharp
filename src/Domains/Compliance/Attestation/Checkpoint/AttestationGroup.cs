namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation group is in its life cycle.</summary>
public enum AttestationGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a attestation group and its current status.</summary>
public sealed class AttestationGroup(string code, string name, double floorValue, string phone, int endsAt, AttestationGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public int EndsAt { get; set; } = endsAt;
    public AttestationGroupStatus Status { get; set; } = status;

    /// <summary>Starts a attestation group in status created.</summary>
    public static AttestationGroup NewAttestationGroup(string code, string name)
    {
        return new AttestationGroup(code, name, 0.0, "", 0, AttestationGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationGroupStatus next)
    {
        switch (Status)
        {
            case AttestationGroupStatus.Created:
                return next == AttestationGroupStatus.Validated || next == AttestationGroupStatus.Retired;
            case AttestationGroupStatus.Validated:
                switch (next)
                {
                    case AttestationGroupStatus.Published:
                    case AttestationGroupStatus.Retired:
                        return true;
                }
                return false;
            case AttestationGroupStatus.Published:
                switch (next)
                {
                    case AttestationGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt - FloorValue, 2);
    }

    public AttestationGroup Copy()
    {
        return new AttestationGroup(Code, Name, FloorValue, Phone, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
