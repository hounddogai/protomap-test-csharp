namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal retro pay review is in its life cycle.</summary>
public enum SeasonalRetroPayReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal retro pay review, with what the planners need to act on it.</summary>
public sealed class SeasonalRetroPayReview(string code, string name, string source, string phone, bool pinned, int leaveDays, double rating, string category, SeasonalRetroPayReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public SeasonalRetroPayReviewStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal retro pay review in status created.</summary>
    public static SeasonalRetroPayReview NewSeasonalRetroPayReview(string code, string name)
    {
        return new SeasonalRetroPayReview(
            code,
            name,
            "",
            "",
            false,
            0,
            0.0,
            "",
            SeasonalRetroPayReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRetroPayReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRetroPayReviewStatus next)
    {
        switch (Status)
        {
            case SeasonalRetroPayReviewStatus.Created:
                return next == SeasonalRetroPayReviewStatus.Validated;
            case SeasonalRetroPayReviewStatus.Validated:
                switch (next)
                {
                    case SeasonalRetroPayReviewStatus.Published:
                    case SeasonalRetroPayReviewStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalRetroPayReviewStatus.Published:
                switch (next)
                {
                    case SeasonalRetroPayReviewStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalRetroPayReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal retro pay review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Rating * (double)LeaveDays, 2);
    }

    public SeasonalRetroPayReview Copy()
    {
        return new SeasonalRetroPayReview(Code, Name, Source, Phone, Pinned, LeaveDays, Rating, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
