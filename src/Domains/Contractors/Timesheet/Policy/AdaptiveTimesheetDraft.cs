namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive timesheet draft is in its life cycle.</summary>
public enum AdaptiveTimesheetDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive timesheet draft as the timesheet team keeps it.</summary>
public sealed class AdaptiveTimesheetDraft(string code, string name, int capacity, string skill, int priority, double threshold, string employeeId, AdaptiveTimesheetDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public AdaptiveTimesheetDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive timesheet draft in status requested.</summary>
    public static AdaptiveTimesheetDraft NewAdaptiveTimesheetDraft(string code, string name)
    {
        return new AdaptiveTimesheetDraft(code, name, 0, "", 0, 0.0, "", AdaptiveTimesheetDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTimesheetDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTimesheetDraftStatus next)
    {
        switch (Status)
        {
            case AdaptiveTimesheetDraftStatus.Requested:
                return next == AdaptiveTimesheetDraftStatus.Expired;
            case AdaptiveTimesheetDraftStatus.Granted:
                switch (next)
                {
                    case AdaptiveTimesheetDraftStatus.Revoked:
                    case AdaptiveTimesheetDraftStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveTimesheetDraftStatus.Revoked:
                switch (next)
                {
                    case AdaptiveTimesheetDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTimesheetDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive timesheet draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive timesheet draft"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold + (double)Priority, 2);
    }

    public AdaptiveTimesheetDraft Copy()
    {
        return new AdaptiveTimesheetDraft(Code, Name, Capacity, Skill, Priority, Threshold, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
