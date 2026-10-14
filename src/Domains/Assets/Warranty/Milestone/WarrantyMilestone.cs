namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty milestone is in its life cycle.</summary>
public enum WarrantyMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A warranty milestone. Its code is unique within a tenant.</summary>
public sealed class WarrantyMilestone(string code, string name, string salaryBand, List<string> tags, string team, double floorValue, double cost, bool pinned, string description, int endsAt, string location, WarrantyMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public List<string> Tags { get; set; } = tags;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public double Cost { get; set; } = cost;
    public bool Pinned { get; set; } = pinned;
    public string Description { get; set; } = description;
    public int EndsAt { get; set; } = endsAt;
    public string Location { get; set; } = location;
    public WarrantyMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a warranty milestone in status requested.</summary>
    public static WarrantyMilestone NewWarrantyMilestone(string code, string name)
    {
        return new WarrantyMilestone(
            code,
            name,
            "",
            new List<string>(),
            "",
            0.0,
            0.0,
            false,
            "",
            0,
            "",
            WarrantyMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyMilestoneStatus next)
    {
        switch (Status)
        {
            case WarrantyMilestoneStatus.Requested:
                switch (next)
                {
                    case WarrantyMilestoneStatus.Granted:
                    case WarrantyMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case WarrantyMilestoneStatus.Granted:
                switch (next)
                {
                    case WarrantyMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case WarrantyMilestoneStatus.Revoked:
                return next == WarrantyMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WarrantyMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"warranty milestone"} {Code}";
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

    public double NetExposure()
    {
        return Math.Round(Cost - FloorValue, 2);
    }

    public WarrantyMilestone Copy()
    {
        return new WarrantyMilestone(
            Code,
            Name,
            SalaryBand,
            Tags,
            Team,
            FloorValue,
            Cost,
            Pinned,
            Description,
            EndsAt,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
