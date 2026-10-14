namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive booking exception is in its life cycle.</summary>
public enum AdaptiveBookingExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive booking exception as the booking team keeps it.</summary>
public sealed class AdaptiveBookingException(string code, string label, double rating, bool active, int capacity, double budget, AdaptiveBookingExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public bool Active { get; set; } = active;
    public int Capacity { get; set; } = capacity;
    public double Budget { get; set; } = budget;
    public AdaptiveBookingExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive booking exception in status requested.</summary>
    public static AdaptiveBookingException NewAdaptiveBookingException(string code, string label)
    {
        return new AdaptiveBookingException(
            code,
            label,
            0.0,
            false,
            0,
            0.0,
            AdaptiveBookingExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBookingExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBookingExceptionStatus next)
    {
        switch (Status)
        {
            case AdaptiveBookingExceptionStatus.Requested:
                return next == AdaptiveBookingExceptionStatus.Revoked || next == AdaptiveBookingExceptionStatus.Expired;
            case AdaptiveBookingExceptionStatus.Granted:
                return next == AdaptiveBookingExceptionStatus.Revoked || next == AdaptiveBookingExceptionStatus.Expired;
            case AdaptiveBookingExceptionStatus.Revoked:
                return next == AdaptiveBookingExceptionStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveBookingExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive booking exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Capacity * Rating, 2);
    }

    public AdaptiveBookingException Copy()
    {
        return new AdaptiveBookingException(Code, Label, Rating, Active, Capacity, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
