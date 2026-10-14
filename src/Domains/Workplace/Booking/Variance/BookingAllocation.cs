namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking allocation is in its life cycle.</summary>
public enum BookingAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One booking allocation, with what the planners need to act on it.</summary>
public sealed class BookingAllocation(string code, string label, string phone, double maximum, double hours, string description, string name, string category, int limit, BookingAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Maximum { get; set; } = maximum;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public int Limit { get; set; } = limit;
    public BookingAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a booking allocation in status idle.</summary>
    public static BookingAllocation NewBookingAllocation(string code, string label)
    {
        return new BookingAllocation(code, label, "", 0.0, 0.0, "", "", "", 0, BookingAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingAllocationStatus next)
    {
        switch (Status)
        {
            case BookingAllocationStatus.Idle:
                switch (next)
                {
                    case BookingAllocationStatus.Warming:
                    case BookingAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case BookingAllocationStatus.Warming:
                return next == BookingAllocationStatus.Draining;
            case BookingAllocationStatus.Ready:
                switch (next)
                {
                    case BookingAllocationStatus.Draining:
                    case BookingAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case BookingAllocationStatus.Draining:
                return next == BookingAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BookingAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Limit * Maximum, 2);
    }

    public BookingAllocation Copy()
    {
        return new BookingAllocation(Code, Label, Phone, Maximum, Hours, Description, Name, Category, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
