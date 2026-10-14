namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped timesheet group is in its life cycle.</summary>
public enum CappedTimesheetGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped timesheet group, with what the planners need to act on it.</summary>
public sealed class CappedTimesheetGroup(string code, string name, int leaveDays, string label, string unit, string channel, string employeeId, Dictionary<string, string> attributes, CappedTimesheetGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Channel { get; set; } = channel;
    public string EmployeeId { get; set; } = employeeId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CappedTimesheetGroupStatus Status { get; set; } = status;

    /// <summary>Starts a capped timesheet group in status created.</summary>
    public static CappedTimesheetGroup NewCappedTimesheetGroup(string code, string name)
    {
        return new CappedTimesheetGroup(
            code,
            name,
            0,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            CappedTimesheetGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTimesheetGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTimesheetGroupStatus next)
    {
        switch (Status)
        {
            case CappedTimesheetGroupStatus.Created:
                return next == CappedTimesheetGroupStatus.Published || next == CappedTimesheetGroupStatus.Retired;
            case CappedTimesheetGroupStatus.Validated:
                switch (next)
                {
                    case CappedTimesheetGroupStatus.Published:
                        return true;
                }
                return false;
            case CappedTimesheetGroupStatus.Published:
                return next == CappedTimesheetGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedTimesheetGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped timesheet group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
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

    public CappedTimesheetGroup Copy()
    {
        return new CappedTimesheetGroup(Code, Name, LeaveDays, Label, Unit, Channel, EmployeeId, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
