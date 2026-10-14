namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request tier is in its life cycle.</summary>
public enum LeaveRequestTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One leave request tier, with what the planners need to act on it.</summary>
public sealed class LeaveRequestTier(string code, string label, double maximum, string grade, int version, double weight, LeaveRequestTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public int Version { get; set; } = version;
    public double Weight { get; set; } = weight;
    public LeaveRequestTierStatus Status { get; set; } = status;

    /// <summary>Starts a leave request tier in status new.</summary>
    public static LeaveRequestTier NewLeaveRequestTier(string code, string label)
    {
        return new LeaveRequestTier(code, label, 0.0, "", 0, 0.0, LeaveRequestTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestTierStatus next)
    {
        switch (Status)
        {
            case LeaveRequestTierStatus.New:
                switch (next)
                {
                    case LeaveRequestTierStatus.Running:
                    case LeaveRequestTierStatus.Failed:
                        return true;
                }
                return false;
            case LeaveRequestTierStatus.Queued:
                return next == LeaveRequestTierStatus.Running;
            case LeaveRequestTierStatus.Running:
                return next == LeaveRequestTierStatus.Done;
            case LeaveRequestTierStatus.Done:
                return next == LeaveRequestTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round(Weight * Maximum, 2);
    }

    public LeaveRequestTier Copy()
    {
        return new LeaveRequestTier(Code, Label, Maximum, Grade, Version, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
