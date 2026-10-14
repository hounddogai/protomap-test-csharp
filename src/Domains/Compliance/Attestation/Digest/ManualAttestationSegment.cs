namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual attestation segment is in its life cycle.</summary>
public enum ManualAttestationSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual attestation segment. Its code is unique within a tenant.</summary>
public sealed class ManualAttestationSegment(string code, string name, Dictionary<string, string> attributes, string team, string externalRef, string badge, double score, double rate, double maximum, List<string> tags, int expiresAt, ManualAttestationSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public ManualAttestationSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual attestation segment in status proposed.</summary>
    public static ManualAttestationSegment NewManualAttestationSegment(string code, string name)
    {
        return new ManualAttestationSegment(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            new List<string>(),
            0,
            ManualAttestationSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAttestationSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAttestationSegmentStatus next)
    {
        switch (Status)
        {
            case ManualAttestationSegmentStatus.Proposed:
                return next == ManualAttestationSegmentStatus.Rejected || next == ManualAttestationSegmentStatus.Withdrawn;
            case ManualAttestationSegmentStatus.Approved:
                return next == ManualAttestationSegmentStatus.Rejected || next == ManualAttestationSegmentStatus.Withdrawn;
            case ManualAttestationSegmentStatus.Rejected:
                return next == ManualAttestationSegmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualAttestationSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual attestation segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double EffectiveExposure()
    {
        return Math.Round(Rate - Maximum, 2);
    }

    public ManualAttestationSegment Copy()
    {
        return new ManualAttestationSegment(
            Code,
            Name,
            Attributes,
            Team,
            ExternalRef,
            Badge,
            Score,
            Rate,
            Maximum,
            Tags,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
