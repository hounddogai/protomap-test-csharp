namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request plan is in its life cycle.</summary>
public enum LeaveRequestPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a leave request plan and its current status.</summary>
public sealed class LeaveRequestPlan(string code, string name, double hours, int startsAt, int capacity, LeaveRequestPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int StartsAt { get; set; } = startsAt;
    public int Capacity { get; set; } = capacity;
    public LeaveRequestPlanStatus Status { get; set; } = status;

    /// <summary>Starts a leave request plan in status requested.</summary>
    public static LeaveRequestPlan NewLeaveRequestPlan(string code, string name)
    {
        return new LeaveRequestPlan(code, name, 0.0, 0, 0, LeaveRequestPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestPlanStatus next)
    {
        switch (Status)
        {
            case LeaveRequestPlanStatus.Requested:
                return next == LeaveRequestPlanStatus.Revoked;
            case LeaveRequestPlanStatus.Granted:
                switch (next)
                {
                    case LeaveRequestPlanStatus.Expired:
                        return true;
                }
                return false;
            case LeaveRequestPlanStatus.Revoked:
                switch (next)
                {
                    case LeaveRequestPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt + (double)Capacity, 2);
    }

    public LeaveRequestPlan Copy()
    {
        return new LeaveRequestPlan(Code, Name, Hours, StartsAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
