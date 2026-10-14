namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation request is in its life cycle.</summary>
public enum AttestationRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a attestation request and its current status.</summary>
public sealed class AttestationRequest(string code, string label, Dictionary<string, string> attributes, double margin, int sequence, int batchSize, string team, int expiresAt, int priority, string ownerId, double minimum, AttestationRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Priority { get; set; } = priority;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public AttestationRequestStatus Status { get; set; } = status;

    /// <summary>Starts a attestation request in status created.</summary>
    public static AttestationRequest NewAttestationRequest(string code, string label)
    {
        return new AttestationRequest(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            0,
            "",
            0,
            0,
            "",
            0.0,
            AttestationRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationRequestStatus next)
    {
        switch (Status)
        {
            case AttestationRequestStatus.Created:
                switch (next)
                {
                    case AttestationRequestStatus.Validated:
                    case AttestationRequestStatus.Retired:
                        return true;
                }
                return false;
            case AttestationRequestStatus.Validated:
                return next == AttestationRequestStatus.Published;
            case AttestationRequestStatus.Published:
                return next == AttestationRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AttestationRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin - (double)Priority, 2);
    }

    public AttestationRequest Copy()
    {
        return new AttestationRequest(
            Code,
            Label,
            Attributes,
            Margin,
            Sequence,
            BatchSize,
            Team,
            ExpiresAt,
            Priority,
            OwnerId,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
