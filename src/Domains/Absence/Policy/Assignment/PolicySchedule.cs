namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy schedule is in its life cycle.</summary>
public enum PolicyScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One policy schedule, with what the planners need to act on it.</summary>
public sealed class PolicySchedule(string code, string title, int windowDays, double amount, string managerId, string channel, bool archived, PolicyScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public PolicyScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a policy schedule in status idle.</summary>
    public static PolicySchedule NewPolicySchedule(string code, string title)
    {
        return new PolicySchedule(code, title, 0, 0.0, "", "", false, PolicyScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyScheduleStatus next)
    {
        switch (Status)
        {
            case PolicyScheduleStatus.Idle:
                return next == PolicyScheduleStatus.Stopped;
            case PolicyScheduleStatus.Warming:
                return next == PolicyScheduleStatus.Ready || next == PolicyScheduleStatus.Draining;
            case PolicyScheduleStatus.Ready:
                switch (next)
                {
                    case PolicyScheduleStatus.Draining:
                    case PolicyScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyScheduleStatus.Draining:
                return next == PolicyScheduleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PolicyScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount * (double)WindowDays, 2);
    }

    public PolicySchedule Copy()
    {
        return new PolicySchedule(Code, Title, WindowDays, Amount, ManagerId, Channel, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
