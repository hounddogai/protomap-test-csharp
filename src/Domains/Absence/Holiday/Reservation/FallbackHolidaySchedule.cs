namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback holiday schedule is in its life cycle.</summary>
public enum FallbackHolidayScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback holiday schedule as the holiday team keeps it.</summary>
public sealed class FallbackHolidaySchedule(string code, string title, int leaveDays, string externalRef, Dictionary<string, string> attributes, string timezone, FallbackHolidayScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Timezone { get; set; } = timezone;
    public FallbackHolidayScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback holiday schedule in status proposed.</summary>
    public static FallbackHolidaySchedule NewFallbackHolidaySchedule(string code, string title)
    {
        return new FallbackHolidaySchedule(
            code,
            title,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            FallbackHolidayScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackHolidayScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackHolidayScheduleStatus next)
    {
        switch (Status)
        {
            case FallbackHolidayScheduleStatus.Proposed:
                switch (next)
                {
                    case FallbackHolidayScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackHolidayScheduleStatus.Approved:
                return next == FallbackHolidayScheduleStatus.Rejected || next == FallbackHolidayScheduleStatus.Withdrawn;
            case FallbackHolidayScheduleStatus.Rejected:
                return next == FallbackHolidayScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackHolidayScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback holiday schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback holiday schedule"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public FallbackHolidaySchedule Copy()
    {
        return new FallbackHolidaySchedule(Code, Title, LeaveDays, ExternalRef, Attributes, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
