namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending span claim is in its life cycle.</summary>
public enum PendingSpanClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending span claim. Its code is unique within a tenant.</summary>
public sealed class PendingSpanClaim(string code, string label, string country, List<string> tags, string description, int updatedAt, string timezone, string grade, int revision, double cost, PendingSpanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Timezone { get; set; } = timezone;
    public string Grade { get; set; } = grade;
    public int Revision { get; set; } = revision;
    public double Cost { get; set; } = cost;
    public PendingSpanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a pending span claim in status requested.</summary>
    public static PendingSpanClaim NewPendingSpanClaim(string code, string label)
    {
        return new PendingSpanClaim(
            code,
            label,
            "",
            new List<string>(),
            "",
            0,
            "",
            "",
            0,
            0.0,
            PendingSpanClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSpanClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSpanClaimStatus next)
    {
        switch (Status)
        {
            case PendingSpanClaimStatus.Requested:
                return next == PendingSpanClaimStatus.Granted || next == PendingSpanClaimStatus.Expired;
            case PendingSpanClaimStatus.Granted:
                switch (next)
                {
                    case PendingSpanClaimStatus.Revoked:
                    case PendingSpanClaimStatus.Expired:
                        return true;
                }
                return false;
            case PendingSpanClaimStatus.Revoked:
                return next == PendingSpanClaimStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingSpanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending span claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pending span claim"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round(Cost + (double)Revision, 2);
    }

    public PendingSpanClaim Copy()
    {
        return new PendingSpanClaim(
            Code,
            Label,
            Country,
            Tags,
            Description,
            UpdatedAt,
            Timezone,
            Grade,
            Revision,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
