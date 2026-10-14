namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking claim is in its life cycle.</summary>
public enum BookingClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking claim. Its code is unique within a tenant.</summary>
public sealed class BookingClaim(string code, string label, string channel, int limit, int attempts, string currency, double ratio, bool archived, int durationMinutes, int expiresAt, BookingClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public int Attempts { get; set; } = attempts;
    public string Currency { get; set; } = currency;
    public double Ratio { get; set; } = ratio;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public BookingClaimStatus Status { get; set; } = status;

    /// <summary>Starts a booking claim in status draft.</summary>
    public static BookingClaim NewBookingClaim(string code, string label)
    {
        return new BookingClaim(code, label, "", 0, 0, "", 0.0, false, 0, 0, BookingClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingClaimStatus next)
    {
        switch (Status)
        {
            case BookingClaimStatus.Draft:
                switch (next)
                {
                    case BookingClaimStatus.Active:
                        return true;
                }
                return false;
            case BookingClaimStatus.Pending:
                return next == BookingClaimStatus.Active || next == BookingClaimStatus.Suspended;
            case BookingClaimStatus.Active:
                switch (next)
                {
                    case BookingClaimStatus.Suspended:
                    case BookingClaimStatus.Closed:
                        return true;
                }
                return false;
            case BookingClaimStatus.Suspended:
                return next == BookingClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BookingClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts + (double)Limit, 2);
    }

    public BookingClaim Copy()
    {
        return new BookingClaim(
            Code,
            Label,
            Channel,
            Limit,
            Attempts,
            Currency,
            Ratio,
            Archived,
            DurationMinutes,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
