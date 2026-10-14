namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request template is in its life cycle.</summary>
public enum LeaveRequestTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One leave request template, with what the planners need to act on it.</summary>
public sealed class LeaveRequestTemplate(string code, string name, int endsAt, string employeeId, bool active, LeaveRequestTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public LeaveRequestTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a leave request template in status requested.</summary>
    public static LeaveRequestTemplate NewLeaveRequestTemplate(string code, string name)
    {
        return new LeaveRequestTemplate(code, name, 0, "", false, LeaveRequestTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestTemplateStatus next)
    {
        switch (Status)
        {
            case LeaveRequestTemplateStatus.Requested:
                switch (next)
                {
                    case LeaveRequestTemplateStatus.Expired:
                        return true;
                }
                return false;
            case LeaveRequestTemplateStatus.Granted:
                switch (next)
                {
                    case LeaveRequestTemplateStatus.Revoked:
                    case LeaveRequestTemplateStatus.Expired:
                        return true;
                }
                return false;
            case LeaveRequestTemplateStatus.Revoked:
                switch (next)
                {
                    case LeaveRequestTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"leave request template"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public LeaveRequestTemplate Copy()
    {
        return new LeaveRequestTemplate(Code, Name, EndsAt, EmployeeId, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
