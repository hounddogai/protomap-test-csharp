namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal visitor schedule is in its life cycle.</summary>
public enum SeasonalVisitorScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal visitor schedule, with what the planners need to act on it.</summary>
public sealed class SeasonalVisitorSchedule(string code, string name, int startsAt, int durationMinutes, int graceDays, string department, string contactEmail, string category, SeasonalVisitorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public string ContactEmail { get; set; } = contactEmail;
    public string Category { get; set; } = category;
    public SeasonalVisitorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal visitor schedule in status created.</summary>
    public static SeasonalVisitorSchedule NewSeasonalVisitorSchedule(string code, string name)
    {
        return new SeasonalVisitorSchedule(code, name, 0, 0, 0, "", "", "", SeasonalVisitorScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalVisitorScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalVisitorScheduleStatus next)
    {
        switch (Status)
        {
            case SeasonalVisitorScheduleStatus.Created:
                switch (next)
                {
                    case SeasonalVisitorScheduleStatus.Published:
                    case SeasonalVisitorScheduleStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalVisitorScheduleStatus.Validated:
                return next == SeasonalVisitorScheduleStatus.Published || next == SeasonalVisitorScheduleStatus.Retired;
            case SeasonalVisitorScheduleStatus.Published:
                switch (next)
                {
                    case SeasonalVisitorScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalVisitorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal visitor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal visitor schedule"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt + (double)GraceDays, 2);
    }

    public SeasonalVisitorSchedule Copy()
    {
        return new SeasonalVisitorSchedule(
            Code,
            Name,
            StartsAt,
            DurationMinutes,
            GraceDays,
            Department,
            ContactEmail,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
