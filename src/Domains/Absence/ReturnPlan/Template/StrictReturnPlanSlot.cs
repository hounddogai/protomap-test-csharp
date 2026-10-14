namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan slot is in its life cycle.</summary>
public enum StrictReturnPlanSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict return plan slot as the return plan team keeps it.</summary>
public sealed class StrictReturnPlanSlot(string code, string title, int sequence, int limit, int quantity, string contactEmail, string country, string costCenter, double hours, StrictReturnPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public string Country { get; set; } = country;
    public string CostCenter { get; set; } = costCenter;
    public double Hours { get; set; } = hours;
    public StrictReturnPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan slot in status proposed.</summary>
    public static StrictReturnPlanSlot NewStrictReturnPlanSlot(string code, string title)
    {
        return new StrictReturnPlanSlot(code, title, 0, 0, 0, "", "", "", 0.0, StrictReturnPlanSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanSlotStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanSlotStatus.Proposed:
                return next == StrictReturnPlanSlotStatus.Approved || next == StrictReturnPlanSlotStatus.Withdrawn;
            case StrictReturnPlanSlotStatus.Approved:
                return next == StrictReturnPlanSlotStatus.Withdrawn;
            case StrictReturnPlanSlotStatus.Rejected:
                return next == StrictReturnPlanSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict return plan slot"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours - (double)Limit, 2);
    }

    public StrictReturnPlanSlot Copy()
    {
        return new StrictReturnPlanSlot(
            Code,
            Title,
            Sequence,
            Limit,
            Quantity,
            ContactEmail,
            Country,
            CostCenter,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
