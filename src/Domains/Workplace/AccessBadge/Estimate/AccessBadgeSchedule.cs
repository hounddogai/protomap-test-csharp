namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge schedule is in its life cycle.</summary>
public enum AccessBadgeScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A access badge schedule. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeSchedule(string code, string title, int sequence, bool archived, string location, string unit, int priority, double rate, string name, double minimum, string department, AccessBadgeScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public AccessBadgeScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a access badge schedule in status created.</summary>
    public static AccessBadgeSchedule NewAccessBadgeSchedule(string code, string title)
    {
        return new AccessBadgeSchedule(
            code,
            title,
            0,
            false,
            "",
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            AccessBadgeScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeScheduleStatus next)
    {
        switch (Status)
        {
            case AccessBadgeScheduleStatus.Created:
                return next == AccessBadgeScheduleStatus.Published;
            case AccessBadgeScheduleStatus.Validated:
                return next == AccessBadgeScheduleStatus.Published;
            case AccessBadgeScheduleStatus.Published:
                switch (next)
                {
                    case AccessBadgeScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Minimum + Rate, 2);
    }

    public AccessBadgeSchedule Copy()
    {
        return new AccessBadgeSchedule(
            Code,
            Title,
            Sequence,
            Archived,
            Location,
            Unit,
            Priority,
            Rate,
            Name,
            Minimum,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
