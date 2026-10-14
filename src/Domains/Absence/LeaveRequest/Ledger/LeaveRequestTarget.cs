namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request target is in its life cycle.</summary>
public enum LeaveRequestTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A leave request target. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestTarget(string code, string label, int version, int sequence, int quantity, int priority, double budget, string segment, string reviewer, string badge, string jobCode, LeaveRequestTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int Sequence { get; set; } = sequence;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public string Reviewer { get; set; } = reviewer;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public LeaveRequestTargetStatus Status { get; set; } = status;

    /// <summary>Starts a leave request target in status planned.</summary>
    public static LeaveRequestTarget NewLeaveRequestTarget(string code, string label)
    {
        return new LeaveRequestTarget(
            code,
            label,
            0,
            0,
            0,
            0,
            0.0,
            "",
            "",
            "",
            "",
            LeaveRequestTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestTargetStatus next)
    {
        switch (Status)
        {
            case LeaveRequestTargetStatus.Planned:
                switch (next)
                {
                    case LeaveRequestTargetStatus.Completed:
                        return true;
                }
                return false;
            case LeaveRequestTargetStatus.Scheduled:
                switch (next)
                {
                    case LeaveRequestTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestTargetStatus.InProgress:
                switch (next)
                {
                    case LeaveRequestTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LeaveRequestTargetStatus.Completed:
                return next == LeaveRequestTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"leave request target"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Budget * (double)Quantity, 2);
    }

    public LeaveRequestTarget Copy()
    {
        return new LeaveRequestTarget(
            Code,
            Label,
            Version,
            Sequence,
            Quantity,
            Priority,
            Budget,
            Segment,
            Reviewer,
            Badge,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
