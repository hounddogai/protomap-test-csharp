namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant claim is in its life cycle.</summary>
public enum EquityGrantClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A equity grant claim. Its code is unique within a tenant.</summary>
public sealed class EquityGrantClaim(string code, string label, string category, string notes, List<string> tags, int updatedAt, int startsAt, string reviewer, EquityGrantClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public int StartsAt { get; set; } = startsAt;
    public string Reviewer { get; set; } = reviewer;
    public EquityGrantClaimStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant claim in status open.</summary>
    public static EquityGrantClaim NewEquityGrantClaim(string code, string label)
    {
        return new EquityGrantClaim(code, label, "", "", new List<string>(), 0, 0, "", EquityGrantClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantClaimStatus next)
    {
        switch (Status)
        {
            case EquityGrantClaimStatus.Open:
                return next == EquityGrantClaimStatus.InReview || next == EquityGrantClaimStatus.Reopened;
            case EquityGrantClaimStatus.InReview:
                switch (next)
                {
                    case EquityGrantClaimStatus.Reopened:
                    case EquityGrantClaimStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantClaimStatus.Resolved:
                return next == EquityGrantClaimStatus.Reopened || next == EquityGrantClaimStatus.Archived;
            case EquityGrantClaimStatus.Reopened:
                switch (next)
                {
                    case EquityGrantClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetValue()
    {
        return Math.Round((double)StartsAt + (double)UpdatedAt, 2);
    }

    public EquityGrantClaim Copy()
    {
        return new EquityGrantClaim(Code, Label, Category, Notes, Tags, UpdatedAt, StartsAt, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
