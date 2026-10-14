namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending laptop reservation is in its life cycle.</summary>
public enum PendingLaptopReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pending laptop reservation and its current status.</summary>
public sealed class PendingLaptopReservation(string code, string title, int priority, string contactEmail, int attempts, string country, int durationMinutes, string category, double discount, PendingLaptopReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Category { get; set; } = category;
    public double Discount { get; set; } = discount;
    public PendingLaptopReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pending laptop reservation in status idle.</summary>
    public static PendingLaptopReservation NewPendingLaptopReservation(string code, string title)
    {
        return new PendingLaptopReservation(
            code,
            title,
            0,
            "",
            0,
            "",
            0,
            "",
            0.0,
            PendingLaptopReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLaptopReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLaptopReservationStatus next)
    {
        switch (Status)
        {
            case PendingLaptopReservationStatus.Idle:
                switch (next)
                {
                    case PendingLaptopReservationStatus.Warming:
                        return true;
                }
                return false;
            case PendingLaptopReservationStatus.Warming:
                switch (next)
                {
                    case PendingLaptopReservationStatus.Draining:
                        return true;
                }
                return false;
            case PendingLaptopReservationStatus.Ready:
                switch (next)
                {
                    case PendingLaptopReservationStatus.Draining:
                    case PendingLaptopReservationStatus.Stopped:
                        return true;
                }
                return false;
            case PendingLaptopReservationStatus.Draining:
                switch (next)
                {
                    case PendingLaptopReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLaptopReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending laptop reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts - (double)Priority, 2);
    }

    public PendingLaptopReservation Copy()
    {
        return new PendingLaptopReservation(
            Code,
            Title,
            Priority,
            ContactEmail,
            Attempts,
            Country,
            DurationMinutes,
            Category,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
