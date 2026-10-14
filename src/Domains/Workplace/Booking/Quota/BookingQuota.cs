namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking quota is in its life cycle.</summary>
public enum BookingQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a booking quota and its current status.</summary>
public sealed class BookingQuota(string code, string title, int capacity, string category, int expiresAt, string timezone, BookingQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public BookingQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a booking quota in status open.</summary>
    public static BookingQuota NewBookingQuota(string code, string title)
    {
        return new BookingQuota(code, title, 0, "", 0, "", BookingQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingQuotaStatus next)
    {
        switch (Status)
        {
            case BookingQuotaStatus.Open:
                switch (next)
                {
                    case BookingQuotaStatus.InReview:
                    case BookingQuotaStatus.Resolved:
                        return true;
                }
                return false;
            case BookingQuotaStatus.InReview:
                return next == BookingQuotaStatus.Reopened;
            case BookingQuotaStatus.Resolved:
                switch (next)
                {
                    case BookingQuotaStatus.Reopened:
                    case BookingQuotaStatus.Archived:
                        return true;
                }
                return false;
            case BookingQuotaStatus.Reopened:
                switch (next)
                {
                    case BookingQuotaStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"booking quota"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Capacity - (double)ExpiresAt, 2);
    }

    public BookingQuota Copy()
    {
        return new BookingQuota(Code, Title, Capacity, Category, ExpiresAt, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
