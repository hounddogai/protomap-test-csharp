namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation allocation is in its life cycle.</summary>
public enum AttestationAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A attestation allocation. Its code is unique within a tenant.</summary>
public sealed class AttestationAllocation(string code, string title, Dictionary<string, string> attributes, double minimum, double score, string location, string contactEmail, int createdAt, double fte, AttestationAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public AttestationAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a attestation allocation in status open.</summary>
    public static AttestationAllocation NewAttestationAllocation(string code, string title)
    {
        return new AttestationAllocation(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            "",
            "",
            0,
            0.0,
            AttestationAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationAllocationStatus next)
    {
        switch (Status)
        {
            case AttestationAllocationStatus.Open:
                switch (next)
                {
                    case AttestationAllocationStatus.Reopened:
                    case AttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
            case AttestationAllocationStatus.InReview:
                return next == AttestationAllocationStatus.Resolved;
            case AttestationAllocationStatus.Resolved:
                switch (next)
                {
                    case AttestationAllocationStatus.Reopened:
                    case AttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
            case AttestationAllocationStatus.Reopened:
                switch (next)
                {
                    case AttestationAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedVolume()
    {
        return Math.Round(Fte + (double)CreatedAt, 2);
    }

    public AttestationAllocation Copy()
    {
        return new AttestationAllocation(
            Code,
            Title,
            Attributes,
            Minimum,
            Score,
            Location,
            ContactEmail,
            CreatedAt,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
