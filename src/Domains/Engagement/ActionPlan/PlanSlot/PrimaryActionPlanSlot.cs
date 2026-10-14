namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary action plan slot is in its life cycle.</summary>
public enum PrimaryActionPlanSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary action plan slot as the action plan team keeps it.</summary>
public sealed class PrimaryActionPlanSlot(string code, string name, string label, string locale, double floorValue, string channel, string costCenter, string segment, int capacity, PrimaryActionPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public PrimaryActionPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a primary action plan slot in status proposed.</summary>
    public static PrimaryActionPlanSlot NewPrimaryActionPlanSlot(string code, string name)
    {
        return new PrimaryActionPlanSlot(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            "",
            0,
            PrimaryActionPlanSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryActionPlanSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryActionPlanSlotStatus next)
    {
        switch (Status)
        {
            case PrimaryActionPlanSlotStatus.Proposed:
                switch (next)
                {
                    case PrimaryActionPlanSlotStatus.Rejected:
                    case PrimaryActionPlanSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case PrimaryActionPlanSlotStatus.Approved:
                return next == PrimaryActionPlanSlotStatus.Rejected || next == PrimaryActionPlanSlotStatus.Withdrawn;
            case PrimaryActionPlanSlotStatus.Rejected:
                switch (next)
                {
                    case PrimaryActionPlanSlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryActionPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary action plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity - FloorValue, 2);
    }

    public PrimaryActionPlanSlot Copy()
    {
        return new PrimaryActionPlanSlot(
            Code,
            Name,
            Label,
            Locale,
            FloorValue,
            Channel,
            CostCenter,
            Segment,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
