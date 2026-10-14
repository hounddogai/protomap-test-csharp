namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday review is in its life cycle.</summary>
public enum HolidayReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a holiday review and its current status.</summary>
public sealed class HolidayReview(string code, string title, string currency, int startsAt, bool active, string jobCode, double score, int endsAt, int expiresAt, HolidayReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int StartsAt { get; set; } = startsAt;
    public bool Active { get; set; } = active;
    public string JobCode { get; set; } = jobCode;
    public double Score { get; set; } = score;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public HolidayReviewStatus Status { get; set; } = status;

    /// <summary>Starts a holiday review in status idle.</summary>
    public static HolidayReview NewHolidayReview(string code, string title)
    {
        return new HolidayReview(code, title, "", 0, false, "", 0.0, 0, 0, HolidayReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayReviewStatus next)
    {
        switch (Status)
        {
            case HolidayReviewStatus.Idle:
                switch (next)
                {
                    case HolidayReviewStatus.Warming:
                    case HolidayReviewStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayReviewStatus.Warming:
                switch (next)
                {
                    case HolidayReviewStatus.Ready:
                    case HolidayReviewStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayReviewStatus.Ready:
                switch (next)
                {
                    case HolidayReviewStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayReviewStatus.Draining:
                return next == HolidayReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HolidayReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"holiday review"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Score + (double)EndsAt, 2);
    }

    public HolidayReview Copy()
    {
        return new HolidayReview(Code, Title, Currency, StartsAt, Active, JobCode, Score, EndsAt, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
