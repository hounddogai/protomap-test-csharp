namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending booking plan is in its life cycle.</summary>
public enum PendingBookingPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending booking plan as the booking team keeps it.</summary>
public sealed class PendingBookingPlan(string code, string label, int sequence, int capacity, int limit, bool archived, PendingBookingPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int Capacity { get; set; } = capacity;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public PendingBookingPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending booking plan in status draft.</summary>
    public static PendingBookingPlan NewPendingBookingPlan(string code, string label)
    {
        return new PendingBookingPlan(code, label, 0, 0, 0, false, PendingBookingPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBookingPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBookingPlanStatus next)
    {
        switch (Status)
        {
            case PendingBookingPlanStatus.Draft:
                switch (next)
                {
                    case PendingBookingPlanStatus.Suspended:
                        return true;
                }
                return false;
            case PendingBookingPlanStatus.Pending:
                switch (next)
                {
                    case PendingBookingPlanStatus.Active:
                    case PendingBookingPlanStatus.Closed:
                        return true;
                }
                return false;
            case PendingBookingPlanStatus.Active:
                switch (next)
                {
                    case PendingBookingPlanStatus.Suspended:
                    case PendingBookingPlanStatus.Closed:
                        return true;
                }
                return false;
            case PendingBookingPlanStatus.Suspended:
                switch (next)
                {
                    case PendingBookingPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingBookingPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending booking plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending booking plan"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit + (double)Sequence, 2);
    }

    public PendingBookingPlan Copy()
    {
        return new PendingBookingPlan(Code, Label, Sequence, Capacity, Limit, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
