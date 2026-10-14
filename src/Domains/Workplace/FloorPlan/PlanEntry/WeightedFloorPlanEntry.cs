namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted floor plan entry is in its life cycle.</summary>
public enum WeightedFloorPlanEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted floor plan entry. Its code is unique within a tenant.</summary>
public sealed class WeightedFloorPlanEntry(string code, string label, string ownerId, string badge, string currency, Dictionary<string, string> attributes, double minimum, double budget, string unit, int expiresAt, WeightedFloorPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public double Budget { get; set; } = budget;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public WeightedFloorPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted floor plan entry in status new.</summary>
    public static WeightedFloorPlanEntry NewWeightedFloorPlanEntry(string code, string label)
    {
        return new WeightedFloorPlanEntry(
            code,
            label,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0.0,
            "",
            0,
            WeightedFloorPlanEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedFloorPlanEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedFloorPlanEntryStatus next)
    {
        switch (Status)
        {
            case WeightedFloorPlanEntryStatus.New:
                switch (next)
                {
                    case WeightedFloorPlanEntryStatus.Queued:
                    case WeightedFloorPlanEntryStatus.Running:
                        return true;
                }
                return false;
            case WeightedFloorPlanEntryStatus.Queued:
                switch (next)
                {
                    case WeightedFloorPlanEntryStatus.Done:
                        return true;
                }
                return false;
            case WeightedFloorPlanEntryStatus.Running:
                switch (next)
                {
                    case WeightedFloorPlanEntryStatus.Done:
                    case WeightedFloorPlanEntryStatus.Failed:
                        return true;
                }
                return false;
            case WeightedFloorPlanEntryStatus.Done:
                return next == WeightedFloorPlanEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedFloorPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted floor plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"weighted floor plan entry"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt - Minimum, 2);
    }

    public WeightedFloorPlanEntry Copy()
    {
        return new WeightedFloorPlanEntry(
            Code,
            Label,
            OwnerId,
            Badge,
            Currency,
            Attributes,
            Minimum,
            Budget,
            Unit,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
