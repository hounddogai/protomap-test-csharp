namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking plan is in its life cycle.</summary>
public enum BookingPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking plan. Its code is unique within a tenant.</summary>
public sealed class BookingPlan(string code, string name, bool archived, int priority, double weight, int revision, BookingPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int Priority { get; set; } = priority;
    public double Weight { get; set; } = weight;
    public int Revision { get; set; } = revision;
    public BookingPlanStatus Status { get; set; } = status;

    /// <summary>Starts a booking plan in status draft.</summary>
    public static BookingPlan NewBookingPlan(string code, string name)
    {
        return new BookingPlan(code, name, false, 0, 0.0, 0, BookingPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingPlanStatus next)
    {
        switch (Status)
        {
            case BookingPlanStatus.Draft:
                switch (next)
                {
                    case BookingPlanStatus.Active:
                    case BookingPlanStatus.Suspended:
                        return true;
                }
                return false;
            case BookingPlanStatus.Pending:
                return next == BookingPlanStatus.Closed;
            case BookingPlanStatus.Active:
                switch (next)
                {
                    case BookingPlanStatus.Suspended:
                    case BookingPlanStatus.Closed:
                        return true;
                }
                return false;
            case BookingPlanStatus.Suspended:
                switch (next)
                {
                    case BookingPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight + (double)Priority, 2);
    }

    public BookingPlan Copy()
    {
        return new BookingPlan(Code, Name, Archived, Priority, Weight, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
