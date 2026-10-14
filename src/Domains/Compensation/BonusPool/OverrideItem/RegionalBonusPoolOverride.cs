namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional bonus pool override is in its life cycle.</summary>
public enum RegionalBonusPoolOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional bonus pool override as the bonus pool team keeps it.</summary>
public sealed class RegionalBonusPoolOverride(string code, string label, string category, double fte, int revision, int retries, string currency, string externalRef, string segment, string unit, int durationMinutes, RegionalBonusPoolOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public int Revision { get; set; } = revision;
    public int Retries { get; set; } = retries;
    public string Currency { get; set; } = currency;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public string Unit { get; set; } = unit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public RegionalBonusPoolOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a regional bonus pool override in status new.</summary>
    public static RegionalBonusPoolOverride NewRegionalBonusPoolOverride(string code, string label)
    {
        return new RegionalBonusPoolOverride(
            code,
            label,
            "",
            0.0,
            0,
            0,
            "",
            "",
            "",
            "",
            0,
            RegionalBonusPoolOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBonusPoolOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBonusPoolOverrideStatus next)
    {
        switch (Status)
        {
            case RegionalBonusPoolOverrideStatus.New:
                switch (next)
                {
                    case RegionalBonusPoolOverrideStatus.Queued:
                    case RegionalBonusPoolOverrideStatus.Running:
                        return true;
                }
                return false;
            case RegionalBonusPoolOverrideStatus.Queued:
                switch (next)
                {
                    case RegionalBonusPoolOverrideStatus.Failed:
                        return true;
                }
                return false;
            case RegionalBonusPoolOverrideStatus.Running:
                switch (next)
                {
                    case RegionalBonusPoolOverrideStatus.Done:
                    case RegionalBonusPoolOverrideStatus.Failed:
                        return true;
                }
                return false;
            case RegionalBonusPoolOverrideStatus.Done:
                switch (next)
                {
                    case RegionalBonusPoolOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalBonusPoolOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional bonus pool override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Retries + (double)Revision, 2);
    }

    public RegionalBonusPoolOverride Copy()
    {
        return new RegionalBonusPoolOverride(
            Code,
            Label,
            Category,
            Fte,
            Revision,
            Retries,
            Currency,
            ExternalRef,
            Segment,
            Unit,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
