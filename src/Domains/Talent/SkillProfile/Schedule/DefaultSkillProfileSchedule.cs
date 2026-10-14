namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile schedule is in its life cycle.</summary>
public enum DefaultSkillProfileScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default skill profile schedule. Its code is unique within a tenant.</summary>
public sealed class DefaultSkillProfileSchedule(string code, string title, double hours, string externalRef, string country, string badge, double rating, DefaultSkillProfileScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string ExternalRef { get; set; } = externalRef;
    public string Country { get; set; } = country;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public DefaultSkillProfileScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile schedule in status proposed.</summary>
    public static DefaultSkillProfileSchedule NewDefaultSkillProfileSchedule(string code, string title)
    {
        return new DefaultSkillProfileSchedule(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0.0,
            DefaultSkillProfileScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileScheduleStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileScheduleStatus.Proposed:
                return next == DefaultSkillProfileScheduleStatus.Approved || next == DefaultSkillProfileScheduleStatus.Rejected;
            case DefaultSkillProfileScheduleStatus.Approved:
                return next == DefaultSkillProfileScheduleStatus.Withdrawn;
            case DefaultSkillProfileScheduleStatus.Rejected:
                return next == DefaultSkillProfileScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default skill profile schedule"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Hours * Rating, 2);
    }

    public DefaultSkillProfileSchedule Copy()
    {
        return new DefaultSkillProfileSchedule(Code, Title, Hours, ExternalRef, Country, Badge, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
