namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy milestone is in its life cycle.</summary>
public enum OccupancyMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A occupancy milestone as the occupancy team keeps it.</summary>
public sealed class OccupancyMilestone(string code, string name, double fte, double amount, int createdAt, double ceiling, int batchSize, string checksum, List<string> tags, double weight, OccupancyMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public string Checksum { get; set; } = checksum;
    public List<string> Tags { get; set; } = tags;
    public double Weight { get; set; } = weight;
    public OccupancyMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy milestone in status draft.</summary>
    public static OccupancyMilestone NewOccupancyMilestone(string code, string name)
    {
        return new OccupancyMilestone(
            code,
            name,
            0.0,
            0.0,
            0,
            0.0,
            0,
            "",
            new List<string>(),
            0.0,
            OccupancyMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyMilestoneStatus next)
    {
        switch (Status)
        {
            case OccupancyMilestoneStatus.Draft:
                switch (next)
                {
                    case OccupancyMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case OccupancyMilestoneStatus.Pending:
                return next == OccupancyMilestoneStatus.Suspended;
            case OccupancyMilestoneStatus.Active:
                switch (next)
                {
                    case OccupancyMilestoneStatus.Suspended:
                    case OccupancyMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case OccupancyMilestoneStatus.Suspended:
                return next == OccupancyMilestoneStatus.Closed;
        }
        return false;
    }

    public void MoveTo(OccupancyMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveValue()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public OccupancyMilestone Copy()
    {
        return new OccupancyMilestone(
            Code,
            Name,
            Fte,
            Amount,
            CreatedAt,
            Ceiling,
            BatchSize,
            Checksum,
            Tags,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
