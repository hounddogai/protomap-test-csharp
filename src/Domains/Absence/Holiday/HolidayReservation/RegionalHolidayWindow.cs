namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional holiday window is in its life cycle.</summary>
public enum RegionalHolidayWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional holiday window and its current status.</summary>
public sealed class RegionalHolidayWindow(string code, string label, string team, string ownerId, double weight, double cost, double floorValue, RegionalHolidayWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public double FloorValue { get; set; } = floorValue;
    public RegionalHolidayWindowStatus Status { get; set; } = status;

    /// <summary>Starts a regional holiday window in status requested.</summary>
    public static RegionalHolidayWindow NewRegionalHolidayWindow(string code, string label)
    {
        return new RegionalHolidayWindow(code, label, "", "", 0.0, 0.0, 0.0, RegionalHolidayWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalHolidayWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalHolidayWindowStatus next)
    {
        switch (Status)
        {
            case RegionalHolidayWindowStatus.Requested:
                return next == RegionalHolidayWindowStatus.Granted;
            case RegionalHolidayWindowStatus.Granted:
                switch (next)
                {
                    case RegionalHolidayWindowStatus.Expired:
                        return true;
                }
                return false;
            case RegionalHolidayWindowStatus.Revoked:
                return next == RegionalHolidayWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalHolidayWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional holiday window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight * FloorValue, 2);
    }

    public RegionalHolidayWindow Copy()
    {
        return new RegionalHolidayWindow(Code, Label, Team, OwnerId, Weight, Cost, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
