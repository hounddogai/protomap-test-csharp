namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request exception is in its life cycle.</summary>
public enum ScheduledLeaveRequestExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled leave request exception. Its code is unique within a tenant.</summary>
public sealed class ScheduledLeaveRequestException(string code, string label, string currency, string team, double threshold, ScheduledLeaveRequestExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public double Threshold { get; set; } = threshold;
    public ScheduledLeaveRequestExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request exception in status created.</summary>
    public static ScheduledLeaveRequestException NewScheduledLeaveRequestException(string code, string label)
    {
        return new ScheduledLeaveRequestException(
            code,
            label,
            "",
            "",
            0.0,
            ScheduledLeaveRequestExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestExceptionStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestExceptionStatus.Created:
                switch (next)
                {
                    case ScheduledLeaveRequestExceptionStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestExceptionStatus.Validated:
                switch (next)
                {
                    case ScheduledLeaveRequestExceptionStatus.Published:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestExceptionStatus.Published:
                switch (next)
                {
                    case ScheduledLeaveRequestExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled leave request exception"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public ScheduledLeaveRequestException Copy()
    {
        return new ScheduledLeaveRequestException(Code, Label, Currency, Team, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
