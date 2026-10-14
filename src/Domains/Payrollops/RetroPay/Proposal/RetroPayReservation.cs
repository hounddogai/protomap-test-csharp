namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay reservation is in its life cycle.</summary>
public enum RetroPayReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A retro pay reservation as the retro pay team keeps it.</summary>
public sealed class RetroPayReservation(string code, string label, string badge, double rating, int capacity, string phone, bool active, bool visible, string locale, string summary, double threshold, RetroPayReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public RetroPayReservationStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay reservation in status idle.</summary>
    public static RetroPayReservation NewRetroPayReservation(string code, string label)
    {
        return new RetroPayReservation(
            code,
            label,
            "",
            0.0,
            0,
            "",
            false,
            false,
            "",
            "",
            0.0,
            RetroPayReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayReservationStatus next)
    {
        switch (Status)
        {
            case RetroPayReservationStatus.Idle:
                switch (next)
                {
                    case RetroPayReservationStatus.Ready:
                    case RetroPayReservationStatus.Stopped:
                        return true;
                }
                return false;
            case RetroPayReservationStatus.Warming:
                return next == RetroPayReservationStatus.Draining || next == RetroPayReservationStatus.Stopped;
            case RetroPayReservationStatus.Ready:
                return next == RetroPayReservationStatus.Draining || next == RetroPayReservationStatus.Stopped;
            case RetroPayReservationStatus.Draining:
                switch (next)
                {
                    case RetroPayReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity * Rating, 2);
    }

    public RetroPayReservation Copy()
    {
        return new RetroPayReservation(
            Code,
            Label,
            Badge,
            Rating,
            Capacity,
            Phone,
            Active,
            Visible,
            Locale,
            Summary,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
