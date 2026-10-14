namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered adjustment mapping is in its life cycle.</summary>
public enum TieredAdjustmentMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered adjustment mapping and its current status.</summary>
public sealed class TieredAdjustmentMapping(string code, string name, int batchSize, int attempts, List<string> tags, int revision, string description, TieredAdjustmentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int Attempts { get; set; } = attempts;
    public List<string> Tags { get; set; } = tags;
    public int Revision { get; set; } = revision;
    public string Description { get; set; } = description;
    public TieredAdjustmentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a tiered adjustment mapping in status new.</summary>
    public static TieredAdjustmentMapping NewTieredAdjustmentMapping(string code, string name)
    {
        return new TieredAdjustmentMapping(
            code,
            name,
            0,
            0,
            new List<string>(),
            0,
            "",
            TieredAdjustmentMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAdjustmentMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAdjustmentMappingStatus next)
    {
        switch (Status)
        {
            case TieredAdjustmentMappingStatus.New:
                switch (next)
                {
                    case TieredAdjustmentMappingStatus.Running:
                        return true;
                }
                return false;
            case TieredAdjustmentMappingStatus.Queued:
                switch (next)
                {
                    case TieredAdjustmentMappingStatus.Done:
                        return true;
                }
                return false;
            case TieredAdjustmentMappingStatus.Running:
                return next == TieredAdjustmentMappingStatus.Done;
            case TieredAdjustmentMappingStatus.Done:
                switch (next)
                {
                    case TieredAdjustmentMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAdjustmentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered adjustment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
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

    public double ProjectedExposure()
    {
        return Math.Round((double)Attempts + (double)Revision, 2);
    }

    public TieredAdjustmentMapping Copy()
    {
        return new TieredAdjustmentMapping(Code, Name, BatchSize, Attempts, Tags, Revision, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
