namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor allocation is in its life cycle.</summary>
public enum VisitorAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A visitor allocation as the visitor team keeps it.</summary>
public sealed class VisitorAllocation(string code, string label, string channel, Dictionary<string, string> attributes, int graceDays, string employeeId, double floorValue, VisitorAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public double FloorValue { get; set; } = floorValue;
    public VisitorAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a visitor allocation in status planned.</summary>
    public static VisitorAllocation NewVisitorAllocation(string code, string label)
    {
        return new VisitorAllocation(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            VisitorAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorAllocationStatus next)
    {
        switch (Status)
        {
            case VisitorAllocationStatus.Planned:
                switch (next)
                {
                    case VisitorAllocationStatus.Completed:
                        return true;
                }
                return false;
            case VisitorAllocationStatus.Scheduled:
                switch (next)
                {
                    case VisitorAllocationStatus.InProgress:
                    case VisitorAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorAllocationStatus.InProgress:
                switch (next)
                {
                    case VisitorAllocationStatus.Completed:
                    case VisitorAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorAllocationStatus.Completed:
                return next == VisitorAllocationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(VisitorAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays + FloorValue, 2);
    }

    public VisitorAllocation Copy()
    {
        return new VisitorAllocation(Code, Label, Channel, Attributes, GraceDays, EmployeeId, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
