namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan tier is in its life cycle.</summary>
public enum SeasonalFloorPlanTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One seasonal floor plan tier, with what the planners need to act on it.</summary>
public sealed class SeasonalFloorPlanTier(string code, string title, double weight, bool active, int batchSize, string category, string externalRef, double threshold, double amount, SeasonalFloorPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public double Amount { get; set; } = amount;
    public SeasonalFloorPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan tier in status draft.</summary>
    public static SeasonalFloorPlanTier NewSeasonalFloorPlanTier(string code, string title)
    {
        return new SeasonalFloorPlanTier(
            code,
            title,
            0.0,
            false,
            0,
            "",
            "",
            0.0,
            0.0,
            SeasonalFloorPlanTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanTierStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanTierStatus.Draft:
                return next == SeasonalFloorPlanTierStatus.Active || next == SeasonalFloorPlanTierStatus.Closed;
            case SeasonalFloorPlanTierStatus.Pending:
                return next == SeasonalFloorPlanTierStatus.Closed;
            case SeasonalFloorPlanTierStatus.Active:
                return next == SeasonalFloorPlanTierStatus.Suspended;
            case SeasonalFloorPlanTierStatus.Suspended:
                return next == SeasonalFloorPlanTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize * Amount, 2);
    }

    public SeasonalFloorPlanTier Copy()
    {
        return new SeasonalFloorPlanTier(
            Code,
            Title,
            Weight,
            Active,
            BatchSize,
            Category,
            ExternalRef,
            Threshold,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
