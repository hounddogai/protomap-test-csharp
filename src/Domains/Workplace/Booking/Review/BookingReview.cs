namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking review is in its life cycle.</summary>
public enum BookingReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking review. Its code is unique within a tenant.</summary>
public sealed class BookingReview(string code, string title, double budget, int expiresAt, string team, string costCenter, string badge, string grade, BookingReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public BookingReviewStatus Status { get; set; } = status;

    /// <summary>Starts a booking review in status draft.</summary>
    public static BookingReview NewBookingReview(string code, string title)
    {
        return new BookingReview(code, title, 0.0, 0, "", "", "", "", BookingReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingReviewStatus next)
    {
        switch (Status)
        {
            case BookingReviewStatus.Draft:
                return next == BookingReviewStatus.Suspended || next == BookingReviewStatus.Closed;
            case BookingReviewStatus.Pending:
                switch (next)
                {
                    case BookingReviewStatus.Active:
                    case BookingReviewStatus.Closed:
                        return true;
                }
                return false;
            case BookingReviewStatus.Active:
                return next == BookingReviewStatus.Suspended || next == BookingReviewStatus.Closed;
            case BookingReviewStatus.Suspended:
                return next == BookingReviewStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BookingReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt * Budget, 2);
    }

    public BookingReview Copy()
    {
        return new BookingReview(Code, Title, Budget, ExpiresAt, Team, CostCenter, Badge, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
