namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled policy item is in its life cycle.</summary>
public enum ScheduledPolicyItemStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled policy item and its current status.</summary>
public sealed class ScheduledPolicyItem(string code, string title, string employeeId, int sequence, int capacity, bool visible, string managerId, int leaveDays, string category, string department, ScheduledPolicyItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int Sequence { get; set; } = sequence;
    public int Capacity { get; set; } = capacity;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public string Department { get; set; } = department;
    public ScheduledPolicyItemStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled policy item in status idle.</summary>
    public static ScheduledPolicyItem NewScheduledPolicyItem(string code, string title)
    {
        return new ScheduledPolicyItem(code, title, "", 0, 0, false, "", 0, "", "", ScheduledPolicyItemStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPolicyItemStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPolicyItemStatus next)
    {
        switch (Status)
        {
            case ScheduledPolicyItemStatus.Idle:
                switch (next)
                {
                    case ScheduledPolicyItemStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPolicyItemStatus.Warming:
                return next == ScheduledPolicyItemStatus.Draining || next == ScheduledPolicyItemStatus.Stopped;
            case ScheduledPolicyItemStatus.Ready:
                switch (next)
                {
                    case ScheduledPolicyItemStatus.Draining:
                    case ScheduledPolicyItemStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPolicyItemStatus.Draining:
                switch (next)
                {
                    case ScheduledPolicyItemStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPolicyItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled policy item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled policy item"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence - (double)Capacity, 2);
    }

    public ScheduledPolicyItem Copy()
    {
        return new ScheduledPolicyItem(
            Code,
            Title,
            EmployeeId,
            Sequence,
            Capacity,
            Visible,
            ManagerId,
            LeaveDays,
            Category,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
