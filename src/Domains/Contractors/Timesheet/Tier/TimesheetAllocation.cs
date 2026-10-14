namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet allocation is in its life cycle.</summary>
public enum TimesheetAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A timesheet allocation as the timesheet team keeps it.</summary>
public sealed class TimesheetAllocation(string code, string label, double weight, string segment, string badge, string timezone, string checksum, TimesheetAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public string Badge { get; set; } = badge;
    public string Timezone { get; set; } = timezone;
    public string Checksum { get; set; } = checksum;
    public TimesheetAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet allocation in status idle.</summary>
    public static TimesheetAllocation NewTimesheetAllocation(string code, string label)
    {
        return new TimesheetAllocation(code, label, 0.0, "", "", "", "", TimesheetAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetAllocationStatus next)
    {
        switch (Status)
        {
            case TimesheetAllocationStatus.Idle:
                switch (next)
                {
                    case TimesheetAllocationStatus.Warming:
                    case TimesheetAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case TimesheetAllocationStatus.Warming:
                return next == TimesheetAllocationStatus.Stopped;
            case TimesheetAllocationStatus.Ready:
                return next == TimesheetAllocationStatus.Draining || next == TimesheetAllocationStatus.Stopped;
            case TimesheetAllocationStatus.Draining:
                switch (next)
                {
                    case TimesheetAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public TimesheetAllocation Copy()
    {
        return new TimesheetAllocation(Code, Label, Weight, Segment, Badge, Timezone, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
