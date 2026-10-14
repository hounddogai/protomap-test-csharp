namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal warranty tier is in its life cycle.</summary>
public enum SeasonalWarrantyTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One seasonal warranty tier, with what the planners need to act on it.</summary>
public sealed class SeasonalWarrantyTier(string code, string name, string department, string summary, bool visible, int attempts, string managerId, string segment, double margin, SeasonalWarrantyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public bool Visible { get; set; } = visible;
    public int Attempts { get; set; } = attempts;
    public string ManagerId { get; set; } = managerId;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public SeasonalWarrantyTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal warranty tier in status new.</summary>
    public static SeasonalWarrantyTier NewSeasonalWarrantyTier(string code, string name)
    {
        return new SeasonalWarrantyTier(code, name, "", "", false, 0, "", "", 0.0, SeasonalWarrantyTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalWarrantyTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalWarrantyTierStatus next)
    {
        switch (Status)
        {
            case SeasonalWarrantyTierStatus.New:
                return next == SeasonalWarrantyTierStatus.Queued;
            case SeasonalWarrantyTierStatus.Queued:
                return next == SeasonalWarrantyTierStatus.Running || next == SeasonalWarrantyTierStatus.Failed;
            case SeasonalWarrantyTierStatus.Running:
                return next == SeasonalWarrantyTierStatus.Done || next == SeasonalWarrantyTierStatus.Failed;
            case SeasonalWarrantyTierStatus.Done:
                switch (next)
                {
                    case SeasonalWarrantyTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalWarrantyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal warranty tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Attempts * Margin, 2);
    }

    public SeasonalWarrantyTier Copy()
    {
        return new SeasonalWarrantyTier(
            Code,
            Name,
            Department,
            Summary,
            Visible,
            Attempts,
            ManagerId,
            Segment,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
