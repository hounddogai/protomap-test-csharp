namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy holiday bundle is in its life cycle.</summary>
public enum LegacyHolidayBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy holiday bundle, with what the planners need to act on it.</summary>
public sealed class LegacyHolidayBundle(string code, string title, string region, string location, bool pinned, double rating, double minimum, double rate, LegacyHolidayBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public LegacyHolidayBundleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy holiday bundle in status open.</summary>
    public static LegacyHolidayBundle NewLegacyHolidayBundle(string code, string title)
    {
        return new LegacyHolidayBundle(code, title, "", "", false, 0.0, 0.0, 0.0, LegacyHolidayBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHolidayBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHolidayBundleStatus next)
    {
        switch (Status)
        {
            case LegacyHolidayBundleStatus.Open:
                return next == LegacyHolidayBundleStatus.Archived;
            case LegacyHolidayBundleStatus.InReview:
                return next == LegacyHolidayBundleStatus.Resolved || next == LegacyHolidayBundleStatus.Reopened;
            case LegacyHolidayBundleStatus.Resolved:
                return next == LegacyHolidayBundleStatus.Reopened || next == LegacyHolidayBundleStatus.Archived;
            case LegacyHolidayBundleStatus.Reopened:
                return next == LegacyHolidayBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyHolidayBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy holiday bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rate - Minimum, 2);
    }

    public LegacyHolidayBundle Copy()
    {
        return new LegacyHolidayBundle(Code, Title, Region, Location, Pinned, Rating, Minimum, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
