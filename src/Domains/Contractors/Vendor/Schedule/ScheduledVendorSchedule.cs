namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled vendor schedule is in its life cycle.</summary>
public enum ScheduledVendorScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One scheduled vendor schedule, with what the planners need to act on it.</summary>
public sealed class ScheduledVendorSchedule(string code, string title, string currency, string name, double rating, int windowDays, double floorValue, string salaryBand, ScheduledVendorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public ScheduledVendorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled vendor schedule in status requested.</summary>
    public static ScheduledVendorSchedule NewScheduledVendorSchedule(string code, string title)
    {
        return new ScheduledVendorSchedule(
            code,
            title,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            ScheduledVendorScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledVendorScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledVendorScheduleStatus next)
    {
        switch (Status)
        {
            case ScheduledVendorScheduleStatus.Requested:
                return next == ScheduledVendorScheduleStatus.Revoked;
            case ScheduledVendorScheduleStatus.Granted:
                return next == ScheduledVendorScheduleStatus.Revoked || next == ScheduledVendorScheduleStatus.Expired;
            case ScheduledVendorScheduleStatus.Revoked:
                return next == ScheduledVendorScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledVendorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled vendor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)WindowDays + Rating, 2);
    }

    public ScheduledVendorSchedule Copy()
    {
        return new ScheduledVendorSchedule(
            Code,
            Title,
            Currency,
            Name,
            Rating,
            WindowDays,
            FloorValue,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
