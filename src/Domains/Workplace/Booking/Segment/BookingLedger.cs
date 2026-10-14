namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking ledger is in its life cycle.</summary>
public enum BookingLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One booking ledger, with what the planners need to act on it.</summary>
public sealed class BookingLedger(string code, string name, string skill, int startsAt, int quantity, int capacity, string label, double margin, double floorValue, int attempts, BookingLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public int StartsAt { get; set; } = startsAt;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public BookingLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a booking ledger in status proposed.</summary>
    public static BookingLedger NewBookingLedger(string code, string name)
    {
        return new BookingLedger(code, name, "", 0, 0, 0, "", 0.0, 0.0, 0, BookingLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingLedgerStatus next)
    {
        switch (Status)
        {
            case BookingLedgerStatus.Proposed:
                switch (next)
                {
                    case BookingLedgerStatus.Approved:
                    case BookingLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case BookingLedgerStatus.Approved:
                switch (next)
                {
                    case BookingLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case BookingLedgerStatus.Rejected:
                switch (next)
                {
                    case BookingLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Margin - (double)StartsAt, 2);
    }

    public BookingLedger Copy()
    {
        return new BookingLedger(
            Code,
            Name,
            Skill,
            StartsAt,
            Quantity,
            Capacity,
            Label,
            Margin,
            FloorValue,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
