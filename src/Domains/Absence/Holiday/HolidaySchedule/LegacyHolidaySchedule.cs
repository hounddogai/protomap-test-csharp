namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy holiday schedule is in its life cycle.</summary>
public enum LegacyHolidayScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One legacy holiday schedule, with what the planners need to act on it.</summary>
public sealed class LegacyHolidaySchedule(string code, string label, int updatedAt, string jobCode, string currency, double weight, double rating, LegacyHolidayScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string JobCode { get; set; } = jobCode;
    public string Currency { get; set; } = currency;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public LegacyHolidayScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy holiday schedule in status requested.</summary>
    public static LegacyHolidaySchedule NewLegacyHolidaySchedule(string code, string label)
    {
        return new LegacyHolidaySchedule(code, label, 0, "", "", 0.0, 0.0, LegacyHolidayScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHolidayScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHolidayScheduleStatus next)
    {
        switch (Status)
        {
            case LegacyHolidayScheduleStatus.Requested:
                return next == LegacyHolidayScheduleStatus.Granted || next == LegacyHolidayScheduleStatus.Revoked;
            case LegacyHolidayScheduleStatus.Granted:
                switch (next)
                {
                    case LegacyHolidayScheduleStatus.Expired:
                        return true;
                }
                return false;
            case LegacyHolidayScheduleStatus.Revoked:
                switch (next)
                {
                    case LegacyHolidayScheduleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyHolidayScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy holiday schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rating + (double)UpdatedAt, 2);
    }

    public LegacyHolidaySchedule Copy()
    {
        return new LegacyHolidaySchedule(Code, Label, UpdatedAt, JobCode, Currency, Weight, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
