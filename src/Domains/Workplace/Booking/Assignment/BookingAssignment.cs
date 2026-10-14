namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking assignment is in its life cycle.</summary>
public enum BookingAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A booking assignment as the booking team keeps it.</summary>
public sealed class BookingAssignment(string code, string name, int endsAt, double cost, string notes, string currency, double weight, int limit, BookingAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public string Currency { get; set; } = currency;
    public double Weight { get; set; } = weight;
    public int Limit { get; set; } = limit;
    public BookingAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a booking assignment in status proposed.</summary>
    public static BookingAssignment NewBookingAssignment(string code, string name)
    {
        return new BookingAssignment(code, name, 0, 0.0, "", "", 0.0, 0, BookingAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingAssignmentStatus next)
    {
        switch (Status)
        {
            case BookingAssignmentStatus.Proposed:
                return next == BookingAssignmentStatus.Approved || next == BookingAssignmentStatus.Withdrawn;
            case BookingAssignmentStatus.Approved:
                return next == BookingAssignmentStatus.Rejected || next == BookingAssignmentStatus.Withdrawn;
            case BookingAssignmentStatus.Rejected:
                switch (next)
                {
                    case BookingAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round((double)EndsAt - Cost, 2);
    }

    public BookingAssignment Copy()
    {
        return new BookingAssignment(Code, Name, EndsAt, Cost, Notes, Currency, Weight, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
