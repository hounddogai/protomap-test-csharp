namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global rate review is in its life cycle.</summary>
public enum GlobalRateReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global rate review, with what the planners need to act on it.</summary>
public sealed class GlobalRateReview(string code, string label, int attempts, int version, string country, GlobalRateReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public string Country { get; set; } = country;
    public GlobalRateReviewStatus Status { get; set; } = status;

    /// <summary>Starts a global rate review in status requested.</summary>
    public static GlobalRateReview NewGlobalRateReview(string code, string label)
    {
        return new GlobalRateReview(code, label, 0, 0, "", GlobalRateReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalRateReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalRateReviewStatus next)
    {
        switch (Status)
        {
            case GlobalRateReviewStatus.Requested:
                return next == GlobalRateReviewStatus.Granted;
            case GlobalRateReviewStatus.Granted:
                return next == GlobalRateReviewStatus.Revoked;
            case GlobalRateReviewStatus.Revoked:
                return next == GlobalRateReviewStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalRateReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global rate review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"global rate review"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Version * (double)Attempts, 2);
    }

    public GlobalRateReview Copy()
    {
        return new GlobalRateReview(Code, Label, Attempts, Version, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
