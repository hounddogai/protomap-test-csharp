namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan tier is in its life cycle.</summary>
public enum FloorPlanTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A floor plan tier. Its code is unique within a tenant.</summary>
public sealed class FloorPlanTier(string code, string name, string description, int durationMinutes, string label, double cost, int updatedAt, bool pinned, string summary, FloorPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Pinned { get; set; } = pinned;
    public string Summary { get; set; } = summary;
    public FloorPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan tier in status created.</summary>
    public static FloorPlanTier NewFloorPlanTier(string code, string name)
    {
        return new FloorPlanTier(code, name, "", 0, "", 0.0, 0, false, "", FloorPlanTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanTierStatus next)
    {
        switch (Status)
        {
            case FloorPlanTierStatus.Created:
                switch (next)
                {
                    case FloorPlanTierStatus.Validated:
                    case FloorPlanTierStatus.Published:
                        return true;
                }
                return false;
            case FloorPlanTierStatus.Validated:
                switch (next)
                {
                    case FloorPlanTierStatus.Published:
                    case FloorPlanTierStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanTierStatus.Published:
                switch (next)
                {
                    case FloorPlanTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Cost - (double)DurationMinutes, 2);
    }

    public FloorPlanTier Copy()
    {
        return new FloorPlanTier(
            Code,
            Name,
            Description,
            DurationMinutes,
            Label,
            Cost,
            UpdatedAt,
            Pinned,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
