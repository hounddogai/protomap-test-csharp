namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday tier is in its life cycle.</summary>
public enum HolidayTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A holiday tier as the holiday team keeps it.</summary>
public sealed class HolidayTier(string code, string title, string employeeId, bool pinned, int graceDays, double rating, bool visible, HolidayTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public bool Pinned { get; set; } = pinned;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public bool Visible { get; set; } = visible;
    public HolidayTierStatus Status { get; set; } = status;

    /// <summary>Starts a holiday tier in status proposed.</summary>
    public static HolidayTier NewHolidayTier(string code, string title)
    {
        return new HolidayTier(code, title, "", false, 0, 0.0, false, HolidayTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayTierStatus next)
    {
        switch (Status)
        {
            case HolidayTierStatus.Proposed:
                return next == HolidayTierStatus.Rejected;
            case HolidayTierStatus.Approved:
                switch (next)
                {
                    case HolidayTierStatus.Rejected:
                    case HolidayTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case HolidayTierStatus.Rejected:
                switch (next)
                {
                    case HolidayTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday tier"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)GraceDays + Rating, 2);
    }

    public HolidayTier Copy()
    {
        return new HolidayTier(Code, Title, EmployeeId, Pinned, GraceDays, Rating, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
