namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual assignment schedule is in its life cycle.</summary>
public enum ManualAssignmentScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual assignment schedule. Its code is unique within a tenant.</summary>
public sealed class ManualAssignmentSchedule(string code, string title, string timezone, bool archived, int durationMinutes, ManualAssignmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ManualAssignmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a manual assignment schedule in status draft.</summary>
    public static ManualAssignmentSchedule NewManualAssignmentSchedule(string code, string title)
    {
        return new ManualAssignmentSchedule(code, title, "", false, 0, ManualAssignmentScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssignmentScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssignmentScheduleStatus next)
    {
        switch (Status)
        {
            case ManualAssignmentScheduleStatus.Draft:
                switch (next)
                {
                    case ManualAssignmentScheduleStatus.Suspended:
                        return true;
                }
                return false;
            case ManualAssignmentScheduleStatus.Pending:
                return next == ManualAssignmentScheduleStatus.Suspended || next == ManualAssignmentScheduleStatus.Closed;
            case ManualAssignmentScheduleStatus.Active:
                return next == ManualAssignmentScheduleStatus.Suspended;
            case ManualAssignmentScheduleStatus.Suspended:
                return next == ManualAssignmentScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualAssignmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual assignment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public ManualAssignmentSchedule Copy()
    {
        return new ManualAssignmentSchedule(Code, Title, Timezone, Archived, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
