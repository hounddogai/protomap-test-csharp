namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary booking tier is in its life cycle.</summary>
public enum PrimaryBookingTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary booking tier, with what the planners need to act on it.</summary>
public sealed class PrimaryBookingTier(string code, string title, string grade, int version, int startsAt, double amount, double threshold, string externalRef, PrimaryBookingTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public double Amount { get; set; } = amount;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public PrimaryBookingTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary booking tier in status draft.</summary>
    public static PrimaryBookingTier NewPrimaryBookingTier(string code, string title)
    {
        return new PrimaryBookingTier(code, title, "", 0, 0, 0.0, 0.0, "", PrimaryBookingTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBookingTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBookingTierStatus next)
    {
        switch (Status)
        {
            case PrimaryBookingTierStatus.Draft:
                switch (next)
                {
                    case PrimaryBookingTierStatus.Pending:
                        return true;
                }
                return false;
            case PrimaryBookingTierStatus.Pending:
                return next == PrimaryBookingTierStatus.Suspended || next == PrimaryBookingTierStatus.Closed;
            case PrimaryBookingTierStatus.Active:
                switch (next)
                {
                    case PrimaryBookingTierStatus.Suspended:
                    case PrimaryBookingTierStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryBookingTierStatus.Suspended:
                return next == PrimaryBookingTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryBookingTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary booking tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round(Amount * Threshold, 2);
    }

    public PrimaryBookingTier Copy()
    {
        return new PrimaryBookingTier(Code, Title, Grade, Version, StartsAt, Amount, Threshold, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
