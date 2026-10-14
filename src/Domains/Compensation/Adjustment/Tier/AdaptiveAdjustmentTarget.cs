namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive adjustment target is in its life cycle.</summary>
public enum AdaptiveAdjustmentTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive adjustment target as the adjustment team keeps it.</summary>
public sealed class AdaptiveAdjustmentTarget(string code, string label, int revision, int leaveDays, int limit, string region, string unit, AdaptiveAdjustmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public string Region { get; set; } = region;
    public string Unit { get; set; } = unit;
    public AdaptiveAdjustmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive adjustment target in status created.</summary>
    public static AdaptiveAdjustmentTarget NewAdaptiveAdjustmentTarget(string code, string label)
    {
        return new AdaptiveAdjustmentTarget(code, label, 0, 0, 0, "", "", AdaptiveAdjustmentTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAdjustmentTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAdjustmentTargetStatus next)
    {
        switch (Status)
        {
            case AdaptiveAdjustmentTargetStatus.Created:
                switch (next)
                {
                    case AdaptiveAdjustmentTargetStatus.Validated:
                    case AdaptiveAdjustmentTargetStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveAdjustmentTargetStatus.Validated:
                return next == AdaptiveAdjustmentTargetStatus.Published || next == AdaptiveAdjustmentTargetStatus.Retired;
            case AdaptiveAdjustmentTargetStatus.Published:
                switch (next)
                {
                    case AdaptiveAdjustmentTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAdjustmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive adjustment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays - (double)Revision, 2);
    }

    public AdaptiveAdjustmentTarget Copy()
    {
        return new AdaptiveAdjustmentTarget(Code, Label, Revision, LeaveDays, Limit, Region, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
