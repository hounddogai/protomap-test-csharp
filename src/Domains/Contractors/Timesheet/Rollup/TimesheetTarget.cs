namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet target is in its life cycle.</summary>
public enum TimesheetTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A timesheet target. Its code is unique within a tenant.</summary>
public sealed class TimesheetTarget(string code, string label, string segment, string phone, int limit, int capacity, string region, Dictionary<string, string> attributes, string country, string grade, double hours, TimesheetTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public int Capacity { get; set; } = capacity;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public string Grade { get; set; } = grade;
    public double Hours { get; set; } = hours;
    public TimesheetTargetStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet target in status created.</summary>
    public static TimesheetTarget NewTimesheetTarget(string code, string label)
    {
        return new TimesheetTarget(
            code,
            label,
            "",
            "",
            0,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            TimesheetTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetTargetStatus next)
    {
        switch (Status)
        {
            case TimesheetTargetStatus.Created:
                return next == TimesheetTargetStatus.Validated;
            case TimesheetTargetStatus.Validated:
                switch (next)
                {
                    case TimesheetTargetStatus.Published:
                    case TimesheetTargetStatus.Retired:
                        return true;
                }
                return false;
            case TimesheetTargetStatus.Published:
                return next == TimesheetTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TimesheetTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"timesheet target"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit - (double)Capacity, 2);
    }

    public TimesheetTarget Copy()
    {
        return new TimesheetTarget(
            Code,
            Label,
            Segment,
            Phone,
            Limit,
            Capacity,
            Region,
            Attributes,
            Country,
            Grade,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
