namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal feedback score is in its life cycle.</summary>
public enum SeasonalFeedbackScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal feedback score as the feedback team keeps it.</summary>
public sealed class SeasonalFeedbackScore(string code, string title, string location, string timezone, double rating, int tenureMonths, string country, SeasonalFeedbackScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public double Rating { get; set; } = rating;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public SeasonalFeedbackScoreStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal feedback score in status idle.</summary>
    public static SeasonalFeedbackScore NewSeasonalFeedbackScore(string code, string title)
    {
        return new SeasonalFeedbackScore(code, title, "", "", 0.0, 0, "", SeasonalFeedbackScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFeedbackScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFeedbackScoreStatus next)
    {
        switch (Status)
        {
            case SeasonalFeedbackScoreStatus.Idle:
                return next == SeasonalFeedbackScoreStatus.Ready || next == SeasonalFeedbackScoreStatus.Draining;
            case SeasonalFeedbackScoreStatus.Warming:
                switch (next)
                {
                    case SeasonalFeedbackScoreStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalFeedbackScoreStatus.Ready:
                switch (next)
                {
                    case SeasonalFeedbackScoreStatus.Draining:
                    case SeasonalFeedbackScoreStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalFeedbackScoreStatus.Draining:
                switch (next)
                {
                    case SeasonalFeedbackScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFeedbackScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal feedback score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal feedback score"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)TenureMonths - Rating, 2);
    }

    public SeasonalFeedbackScore Copy()
    {
        return new SeasonalFeedbackScore(Code, Title, Location, Timezone, Rating, TenureMonths, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
