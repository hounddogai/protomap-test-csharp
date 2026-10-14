namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday bundle is in its life cycle.</summary>
public enum HolidayBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One holiday bundle, with what the planners need to act on it.</summary>
public sealed class HolidayBundle(string code, string name, string country, int quantity, bool visible, string unit, string badge, HolidayBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public string Unit { get; set; } = unit;
    public string Badge { get; set; } = badge;
    public HolidayBundleStatus Status { get; set; } = status;

    /// <summary>Starts a holiday bundle in status open.</summary>
    public static HolidayBundle NewHolidayBundle(string code, string name)
    {
        return new HolidayBundle(code, name, "", 0, false, "", "", HolidayBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayBundleStatus next)
    {
        switch (Status)
        {
            case HolidayBundleStatus.Open:
                switch (next)
                {
                    case HolidayBundleStatus.Archived:
                        return true;
                }
                return false;
            case HolidayBundleStatus.InReview:
                switch (next)
                {
                    case HolidayBundleStatus.Reopened:
                        return true;
                }
                return false;
            case HolidayBundleStatus.Resolved:
                switch (next)
                {
                    case HolidayBundleStatus.Archived:
                        return true;
                }
                return false;
            case HolidayBundleStatus.Reopened:
                switch (next)
                {
                    case HolidayBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public HolidayBundle Copy()
    {
        return new HolidayBundle(Code, Name, Country, Quantity, Visible, Unit, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
