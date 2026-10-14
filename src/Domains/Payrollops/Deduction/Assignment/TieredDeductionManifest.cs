namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered deduction manifest is in its life cycle.</summary>
public enum TieredDeductionManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered deduction manifest and its current status.</summary>
public sealed class TieredDeductionManifest(string code, string label, int durationMinutes, double fte, double rate, string team, List<string> tags, double floorValue, string ownerId, double margin, TieredDeductionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public string OwnerId { get; set; } = ownerId;
    public double Margin { get; set; } = margin;
    public TieredDeductionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered deduction manifest in status new.</summary>
    public static TieredDeductionManifest NewTieredDeductionManifest(string code, string label)
    {
        return new TieredDeductionManifest(
            code,
            label,
            0,
            0.0,
            0.0,
            "",
            new List<string>(),
            0.0,
            "",
            0.0,
            TieredDeductionManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredDeductionManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredDeductionManifestStatus next)
    {
        switch (Status)
        {
            case TieredDeductionManifestStatus.New:
                return next == TieredDeductionManifestStatus.Failed;
            case TieredDeductionManifestStatus.Queued:
                return next == TieredDeductionManifestStatus.Running || next == TieredDeductionManifestStatus.Failed;
            case TieredDeductionManifestStatus.Running:
                switch (next)
                {
                    case TieredDeductionManifestStatus.Failed:
                        return true;
                }
                return false;
            case TieredDeductionManifestStatus.Done:
                return next == TieredDeductionManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TieredDeductionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered deduction manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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
        return Math.Round(Margin * (double)DurationMinutes, 2);
    }

    public TieredDeductionManifest Copy()
    {
        return new TieredDeductionManifest(
            Code,
            Label,
            DurationMinutes,
            Fte,
            Rate,
            Team,
            Tags,
            FloorValue,
            OwnerId,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
