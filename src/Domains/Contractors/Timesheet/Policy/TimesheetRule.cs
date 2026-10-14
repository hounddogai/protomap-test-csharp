namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet rule is in its life cycle.</summary>
public enum TimesheetRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a timesheet rule and its current status.</summary>
public sealed class TimesheetRule(string code, string title, double margin, string timezone, int startsAt, TimesheetRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public int StartsAt { get; set; } = startsAt;
    public TimesheetRuleStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet rule in status draft.</summary>
    public static TimesheetRule NewTimesheetRule(string code, string title)
    {
        return new TimesheetRule(code, title, 0.0, "", 0, TimesheetRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetRuleStatus next)
    {
        switch (Status)
        {
            case TimesheetRuleStatus.Draft:
                switch (next)
                {
                    case TimesheetRuleStatus.Pending:
                        return true;
                }
                return false;
            case TimesheetRuleStatus.Pending:
                return next == TimesheetRuleStatus.Active || next == TimesheetRuleStatus.Suspended;
            case TimesheetRuleStatus.Active:
                switch (next)
                {
                    case TimesheetRuleStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetRuleStatus.Suspended:
                return next == TimesheetRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin - (double)StartsAt, 2);
    }

    public TimesheetRule Copy()
    {
        return new TimesheetRule(Code, Title, Margin, Timezone, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
