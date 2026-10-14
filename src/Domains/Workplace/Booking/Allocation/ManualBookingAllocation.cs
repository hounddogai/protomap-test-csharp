namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual booking allocation is in its life cycle.</summary>
public enum ManualBookingAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual booking allocation and its current status.</summary>
public sealed class ManualBookingAllocation(string code, string label, int capacity, double budget, string currency, string category, ManualBookingAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Budget { get; set; } = budget;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public ManualBookingAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a manual booking allocation in status proposed.</summary>
    public static ManualBookingAllocation NewManualBookingAllocation(string code, string label)
    {
        return new ManualBookingAllocation(code, label, 0, 0.0, "", "", ManualBookingAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBookingAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBookingAllocationStatus next)
    {
        switch (Status)
        {
            case ManualBookingAllocationStatus.Proposed:
                return next == ManualBookingAllocationStatus.Rejected;
            case ManualBookingAllocationStatus.Approved:
                switch (next)
                {
                    case ManualBookingAllocationStatus.Rejected:
                    case ManualBookingAllocationStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualBookingAllocationStatus.Rejected:
                return next == ManualBookingAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualBookingAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual booking allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Budget + (double)Capacity, 2);
    }

    public ManualBookingAllocation Copy()
    {
        return new ManualBookingAllocation(Code, Label, Capacity, Budget, Currency, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
