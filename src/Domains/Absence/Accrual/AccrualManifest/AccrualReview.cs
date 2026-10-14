namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual review is in its life cycle.</summary>
public enum AccrualReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a accrual review and its current status.</summary>
public sealed class AccrualReview(string code, string label, string ownerId, string currency, string source, double hours, bool active, double cost, string timezone, int durationMinutes, AccrualReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public string Source { get; set; } = source;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AccrualReviewStatus Status { get; set; } = status;

    /// <summary>Starts a accrual review in status created.</summary>
    public static AccrualReview NewAccrualReview(string code, string label)
    {
        return new AccrualReview(code, label, "", "", "", 0.0, false, 0.0, "", 0, AccrualReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualReviewStatus next)
    {
        switch (Status)
        {
            case AccrualReviewStatus.Created:
                switch (next)
                {
                    case AccrualReviewStatus.Published:
                    case AccrualReviewStatus.Retired:
                        return true;
                }
                return false;
            case AccrualReviewStatus.Validated:
                switch (next)
                {
                    case AccrualReviewStatus.Published:
                        return true;
                }
                return false;
            case AccrualReviewStatus.Published:
                return next == AccrualReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AccrualReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"accrual review"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Hours - (double)DurationMinutes, 2);
    }

    public AccrualReview Copy()
    {
        return new AccrualReview(
            Code,
            Label,
            OwnerId,
            Currency,
            Source,
            Hours,
            Active,
            Cost,
            Timezone,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
