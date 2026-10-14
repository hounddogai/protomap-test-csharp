namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional engagement override is in its life cycle.</summary>
public enum RegionalEngagementOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional engagement override. Its code is unique within a tenant.</summary>
public sealed class RegionalEngagementOverride(string code, string name, int quantity, int updatedAt, bool approved, string location, string segment, int endsAt, RegionalEngagementOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public RegionalEngagementOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a regional engagement override in status open.</summary>
    public static RegionalEngagementOverride NewRegionalEngagementOverride(string code, string name)
    {
        return new RegionalEngagementOverride(
            code,
            name,
            0,
            0,
            false,
            "",
            "",
            0,
            RegionalEngagementOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalEngagementOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalEngagementOverrideStatus next)
    {
        switch (Status)
        {
            case RegionalEngagementOverrideStatus.Open:
                return next == RegionalEngagementOverrideStatus.InReview || next == RegionalEngagementOverrideStatus.Resolved;
            case RegionalEngagementOverrideStatus.InReview:
                return next == RegionalEngagementOverrideStatus.Reopened || next == RegionalEngagementOverrideStatus.Archived;
            case RegionalEngagementOverrideStatus.Resolved:
                return next == RegionalEngagementOverrideStatus.Reopened || next == RegionalEngagementOverrideStatus.Archived;
            case RegionalEngagementOverrideStatus.Reopened:
                switch (next)
                {
                    case RegionalEngagementOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalEngagementOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional engagement override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Quantity + (double)UpdatedAt, 2);
    }

    public RegionalEngagementOverride Copy()
    {
        return new RegionalEngagementOverride(
            Code,
            Name,
            Quantity,
            UpdatedAt,
            Approved,
            Location,
            Segment,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
