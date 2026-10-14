namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy warranty baseline is in its life cycle.</summary>
public enum LegacyWarrantyBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy warranty baseline. Its code is unique within a tenant.</summary>
public sealed class LegacyWarrantyBaseline(string code, string label, int capacity, string location, int version, bool archived, int durationMinutes, string region, double amount, string timezone, string country, LegacyWarrantyBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Location { get; set; } = location;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Region { get; set; } = region;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public string Country { get; set; } = country;
    public LegacyWarrantyBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a legacy warranty baseline in status open.</summary>
    public static LegacyWarrantyBaseline NewLegacyWarrantyBaseline(string code, string label)
    {
        return new LegacyWarrantyBaseline(
            code,
            label,
            0,
            "",
            0,
            false,
            0,
            "",
            0.0,
            "",
            "",
            LegacyWarrantyBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyWarrantyBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyWarrantyBaselineStatus next)
    {
        switch (Status)
        {
            case LegacyWarrantyBaselineStatus.Open:
                switch (next)
                {
                    case LegacyWarrantyBaselineStatus.Archived:
                        return true;
                }
                return false;
            case LegacyWarrantyBaselineStatus.InReview:
                switch (next)
                {
                    case LegacyWarrantyBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyWarrantyBaselineStatus.Resolved:
                switch (next)
                {
                    case LegacyWarrantyBaselineStatus.Archived:
                        return true;
                }
                return false;
            case LegacyWarrantyBaselineStatus.Reopened:
                return next == LegacyWarrantyBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyWarrantyBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy warranty baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"legacy warranty baseline"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Amount * (double)DurationMinutes, 2);
    }

    public LegacyWarrantyBaseline Copy()
    {
        return new LegacyWarrantyBaseline(
            Code,
            Label,
            Capacity,
            Location,
            Version,
            Archived,
            DurationMinutes,
            Region,
            Amount,
            Timezone,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
