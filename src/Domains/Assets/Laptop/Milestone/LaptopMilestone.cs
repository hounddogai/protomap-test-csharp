namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop milestone is in its life cycle.</summary>
public enum LaptopMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One laptop milestone, with what the planners need to act on it.</summary>
public sealed class LaptopMilestone(string code, string title, string costCenter, string description, string department, int quantity, LaptopMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public string Department { get; set; } = department;
    public int Quantity { get; set; } = quantity;
    public LaptopMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a laptop milestone in status planned.</summary>
    public static LaptopMilestone NewLaptopMilestone(string code, string title)
    {
        return new LaptopMilestone(code, title, "", "", "", 0, LaptopMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopMilestoneStatus next)
    {
        switch (Status)
        {
            case LaptopMilestoneStatus.Planned:
                switch (next)
                {
                    case LaptopMilestoneStatus.InProgress:
                        return true;
                }
                return false;
            case LaptopMilestoneStatus.Scheduled:
                switch (next)
                {
                    case LaptopMilestoneStatus.InProgress:
                        return true;
                }
                return false;
            case LaptopMilestoneStatus.InProgress:
                return next == LaptopMilestoneStatus.Cancelled;
            case LaptopMilestoneStatus.Completed:
                return next == LaptopMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LaptopMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public LaptopMilestone Copy()
    {
        return new LaptopMilestone(Code, Title, CostCenter, Description, Department, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
