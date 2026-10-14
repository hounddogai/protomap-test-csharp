namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday baseline is in its life cycle.</summary>
public enum HolidayBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One holiday baseline, with what the planners need to act on it.</summary>
public sealed class HolidayBaseline(string code, string title, string costCenter, double floorValue, int createdAt, int windowDays, double ceiling, int quantity, string team, HolidayBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public double Ceiling { get; set; } = ceiling;
    public int Quantity { get; set; } = quantity;
    public string Team { get; set; } = team;
    public HolidayBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a holiday baseline in status requested.</summary>
    public static HolidayBaseline NewHolidayBaseline(string code, string title)
    {
        return new HolidayBaseline(code, title, "", 0.0, 0, 0, 0.0, 0, "", HolidayBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayBaselineStatus next)
    {
        switch (Status)
        {
            case HolidayBaselineStatus.Requested:
                switch (next)
                {
                    case HolidayBaselineStatus.Revoked:
                    case HolidayBaselineStatus.Expired:
                        return true;
                }
                return false;
            case HolidayBaselineStatus.Granted:
                return next == HolidayBaselineStatus.Revoked;
            case HolidayBaselineStatus.Revoked:
                switch (next)
                {
                    case HolidayBaselineStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling * (double)WindowDays, 2);
    }

    public HolidayBaseline Copy()
    {
        return new HolidayBaseline(
            Code,
            Title,
            CostCenter,
            FloorValue,
            CreatedAt,
            WindowDays,
            Ceiling,
            Quantity,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
