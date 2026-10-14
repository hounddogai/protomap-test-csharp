namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet template is in its life cycle.</summary>
public enum TimesheetTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A timesheet template. Its code is unique within a tenant.</summary>
public sealed class TimesheetTemplate(string code, string name, int endsAt, double fte, string segment, string source, string skill, string grade, TimesheetTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Fte { get; set; } = fte;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public TimesheetTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet template in status draft.</summary>
    public static TimesheetTemplate NewTimesheetTemplate(string code, string name)
    {
        return new TimesheetTemplate(code, name, 0, 0.0, "", "", "", "", TimesheetTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetTemplateStatus next)
    {
        switch (Status)
        {
            case TimesheetTemplateStatus.Draft:
                switch (next)
                {
                    case TimesheetTemplateStatus.Pending:
                    case TimesheetTemplateStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetTemplateStatus.Pending:
                return next == TimesheetTemplateStatus.Suspended;
            case TimesheetTemplateStatus.Active:
                return next == TimesheetTemplateStatus.Suspended || next == TimesheetTemplateStatus.Closed;
            case TimesheetTemplateStatus.Suspended:
                switch (next)
                {
                    case TimesheetTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Fte - (double)EndsAt, 2);
    }

    public TimesheetTemplate Copy()
    {
        return new TimesheetTemplate(Code, Name, EndsAt, Fte, Segment, Source, Skill, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
