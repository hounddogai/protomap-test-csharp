namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled timesheet group is in its life cycle.</summary>
public enum ScheduledTimesheetGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled timesheet group. Its code is unique within a tenant.</summary>
public sealed class ScheduledTimesheetGroup(string code, string label, Dictionary<string, string> attributes, string skill, double hours, int endsAt, double threshold, ScheduledTimesheetGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public int EndsAt { get; set; } = endsAt;
    public double Threshold { get; set; } = threshold;
    public ScheduledTimesheetGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled timesheet group in status proposed.</summary>
    public static ScheduledTimesheetGroup NewScheduledTimesheetGroup(string code, string label)
    {
        return new ScheduledTimesheetGroup(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0.0,
            0,
            0.0,
            ScheduledTimesheetGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTimesheetGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTimesheetGroupStatus next)
    {
        switch (Status)
        {
            case ScheduledTimesheetGroupStatus.Proposed:
                switch (next)
                {
                    case ScheduledTimesheetGroupStatus.Approved:
                        return true;
                }
                return false;
            case ScheduledTimesheetGroupStatus.Approved:
                switch (next)
                {
                    case ScheduledTimesheetGroupStatus.Rejected:
                        return true;
                }
                return false;
            case ScheduledTimesheetGroupStatus.Rejected:
                switch (next)
                {
                    case ScheduledTimesheetGroupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTimesheetGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled timesheet group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scheduled timesheet group"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)EndsAt + Threshold, 2);
    }

    public ScheduledTimesheetGroup Copy()
    {
        return new ScheduledTimesheetGroup(Code, Label, Attributes, Skill, Hours, EndsAt, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
