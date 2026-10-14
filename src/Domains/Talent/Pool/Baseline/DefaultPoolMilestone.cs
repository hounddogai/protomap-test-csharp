namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pool milestone is in its life cycle.</summary>
public enum DefaultPoolMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A default pool milestone. Its code is unique within a tenant.</summary>
public sealed class DefaultPoolMilestone(string code, string name, string region, string description, string locale, double threshold, double floorValue, int createdAt, string category, DefaultPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Description { get; set; } = description;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public DefaultPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a default pool milestone in status requested.</summary>
    public static DefaultPoolMilestone NewDefaultPoolMilestone(string code, string name)
    {
        return new DefaultPoolMilestone(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            DefaultPoolMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPoolMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case DefaultPoolMilestoneStatus.Requested:
                switch (next)
                {
                    case DefaultPoolMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case DefaultPoolMilestoneStatus.Granted:
                return next == DefaultPoolMilestoneStatus.Revoked;
            case DefaultPoolMilestoneStatus.Revoked:
                switch (next)
                {
                    case DefaultPoolMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold - (double)CreatedAt, 2);
    }

    public DefaultPoolMilestone Copy()
    {
        return new DefaultPoolMilestone(
            Code,
            Name,
            Region,
            Description,
            Locale,
            Threshold,
            FloorValue,
            CreatedAt,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
