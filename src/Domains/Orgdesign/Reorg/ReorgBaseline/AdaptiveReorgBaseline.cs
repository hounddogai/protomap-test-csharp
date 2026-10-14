namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive reorg baseline is in its life cycle.</summary>
public enum AdaptiveReorgBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive reorg baseline as the reorg team keeps it.</summary>
public sealed class AdaptiveReorgBaseline(string code, string name, string timezone, string category, int leaveDays, double floorValue, string unit, int retries, bool pinned, AdaptiveReorgBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Category { get; set; } = category;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public bool Pinned { get; set; } = pinned;
    public AdaptiveReorgBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive reorg baseline in status requested.</summary>
    public static AdaptiveReorgBaseline NewAdaptiveReorgBaseline(string code, string name)
    {
        return new AdaptiveReorgBaseline(
            code,
            name,
            "",
            "",
            0,
            0.0,
            "",
            0,
            false,
            AdaptiveReorgBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReorgBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReorgBaselineStatus next)
    {
        switch (Status)
        {
            case AdaptiveReorgBaselineStatus.Requested:
                switch (next)
                {
                    case AdaptiveReorgBaselineStatus.Revoked:
                    case AdaptiveReorgBaselineStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveReorgBaselineStatus.Granted:
                switch (next)
                {
                    case AdaptiveReorgBaselineStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveReorgBaselineStatus.Revoked:
                return next == AdaptiveReorgBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveReorgBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive reorg baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays + (double)Retries, 2);
    }

    public AdaptiveReorgBaseline Copy()
    {
        return new AdaptiveReorgBaseline(
            Code,
            Name,
            Timezone,
            Category,
            LeaveDays,
            FloorValue,
            Unit,
            Retries,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
