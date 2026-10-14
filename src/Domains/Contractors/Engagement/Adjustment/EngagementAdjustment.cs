namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement adjustment is in its life cycle.</summary>
public enum EngagementAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a engagement adjustment and its current status.</summary>
public sealed class EngagementAdjustment(string code, string title, string externalRef, int startsAt, string category, string description, double maximum, int tenureMonths, int expiresAt, EngagementAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public int TenureMonths { get; set; } = tenureMonths;
    public int ExpiresAt { get; set; } = expiresAt;
    public EngagementAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a engagement adjustment in status draft.</summary>
    public static EngagementAdjustment NewEngagementAdjustment(string code, string title)
    {
        return new EngagementAdjustment(code, title, "", 0, "", "", 0.0, 0, 0, EngagementAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementAdjustmentStatus next)
    {
        switch (Status)
        {
            case EngagementAdjustmentStatus.Draft:
                switch (next)
                {
                    case EngagementAdjustmentStatus.Pending:
                        return true;
                }
                return false;
            case EngagementAdjustmentStatus.Pending:
                switch (next)
                {
                    case EngagementAdjustmentStatus.Active:
                    case EngagementAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case EngagementAdjustmentStatus.Active:
                return next == EngagementAdjustmentStatus.Suspended || next == EngagementAdjustmentStatus.Closed;
            case EngagementAdjustmentStatus.Suspended:
                return next == EngagementAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(EngagementAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)ExpiresAt + Maximum, 2);
    }

    public EngagementAdjustment Copy()
    {
        return new EngagementAdjustment(
            Code,
            Title,
            ExternalRef,
            StartsAt,
            Category,
            Description,
            Maximum,
            TenureMonths,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
