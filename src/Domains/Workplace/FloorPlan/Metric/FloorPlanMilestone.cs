namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan milestone is in its life cycle.</summary>
public enum FloorPlanMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a floor plan milestone and its current status.</summary>
public sealed class FloorPlanMilestone(string code, string name, bool visible, string phone, int capacity, int leaveDays, string channel, double discount, FloorPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public FloorPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan milestone in status requested.</summary>
    public static FloorPlanMilestone NewFloorPlanMilestone(string code, string name)
    {
        return new FloorPlanMilestone(code, name, false, "", 0, 0, "", 0.0, FloorPlanMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case FloorPlanMilestoneStatus.Requested:
                switch (next)
                {
                    case FloorPlanMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case FloorPlanMilestoneStatus.Granted:
                switch (next)
                {
                    case FloorPlanMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case FloorPlanMilestoneStatus.Revoked:
                switch (next)
                {
                    case FloorPlanMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays + (double)Capacity, 2);
    }

    public FloorPlanMilestone Copy()
    {
        return new FloorPlanMilestone(Code, Name, Visible, Phone, Capacity, LeaveDays, Channel, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
