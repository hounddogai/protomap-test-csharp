namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay review is in its life cycle.</summary>
public enum RetroPayReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A retro pay review. Its code is unique within a tenant.</summary>
public sealed class RetroPayReview(string code, string title, int durationMinutes, string timezone, int attempts, string region, RetroPayReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Timezone { get; set; } = timezone;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public RetroPayReviewStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay review in status created.</summary>
    public static RetroPayReview NewRetroPayReview(string code, string title)
    {
        return new RetroPayReview(code, title, 0, "", 0, "", RetroPayReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayReviewStatus next)
    {
        switch (Status)
        {
            case RetroPayReviewStatus.Created:
                return next == RetroPayReviewStatus.Retired;
            case RetroPayReviewStatus.Validated:
                return next == RetroPayReviewStatus.Published || next == RetroPayReviewStatus.Retired;
            case RetroPayReviewStatus.Published:
                return next == RetroPayReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts * (double)DurationMinutes, 2);
    }

    public RetroPayReview Copy()
    {
        return new RetroPayReview(Code, Title, DurationMinutes, Timezone, Attempts, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
