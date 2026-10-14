namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday schedule is in its life cycle.</summary>
public enum HolidayScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A holiday schedule as the holiday team keeps it.</summary>
public sealed class HolidaySchedule(string code, string title, string channel, string source, double ratio, HolidayScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public double Ratio { get; set; } = ratio;
    public HolidayScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a holiday schedule in status proposed.</summary>
    public static HolidaySchedule NewHolidaySchedule(string code, string title)
    {
        return new HolidaySchedule(code, title, "", "", 0.0, HolidayScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayScheduleStatus next)
    {
        switch (Status)
        {
            case HolidayScheduleStatus.Proposed:
                return next == HolidayScheduleStatus.Approved || next == HolidayScheduleStatus.Rejected;
            case HolidayScheduleStatus.Approved:
                return next == HolidayScheduleStatus.Rejected;
            case HolidayScheduleStatus.Rejected:
                return next == HolidayScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(HolidayScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public HolidaySchedule Copy()
    {
        return new HolidaySchedule(Code, Title, Channel, Source, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
