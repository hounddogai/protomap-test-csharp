namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict holiday allocation is in its life cycle.</summary>
public enum StrictHolidayAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict holiday allocation and its current status.</summary>
public sealed class StrictHolidayAllocation(string code, string name, int createdAt, int capacity, int updatedAt, int leaveDays, string category, string unit, StrictHolidayAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public int UpdatedAt { get; set; } = updatedAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public StrictHolidayAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a strict holiday allocation in status proposed.</summary>
    public static StrictHolidayAllocation NewStrictHolidayAllocation(string code, string name)
    {
        return new StrictHolidayAllocation(code, name, 0, 0, 0, 0, "", "", StrictHolidayAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictHolidayAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictHolidayAllocationStatus next)
    {
        switch (Status)
        {
            case StrictHolidayAllocationStatus.Proposed:
                return next == StrictHolidayAllocationStatus.Approved || next == StrictHolidayAllocationStatus.Withdrawn;
            case StrictHolidayAllocationStatus.Approved:
                return next == StrictHolidayAllocationStatus.Withdrawn;
            case StrictHolidayAllocationStatus.Rejected:
                return next == StrictHolidayAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictHolidayAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict holiday allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt * (double)UpdatedAt, 2);
    }

    public StrictHolidayAllocation Copy()
    {
        return new StrictHolidayAllocation(
            Code,
            Name,
            CreatedAt,
            Capacity,
            UpdatedAt,
            LeaveDays,
            Category,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
