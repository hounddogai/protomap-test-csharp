namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan review is in its life cycle.</summary>
public enum AdaptiveActionPlanReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adaptive action plan review, with what the planners need to act on it.</summary>
public sealed class AdaptiveActionPlanReview(string code, string title, string channel, string location, double threshold, double score, AdaptiveActionPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public double Score { get; set; } = score;
    public AdaptiveActionPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan review in status created.</summary>
    public static AdaptiveActionPlanReview NewAdaptiveActionPlanReview(string code, string title)
    {
        return new AdaptiveActionPlanReview(code, title, "", "", 0.0, 0.0, AdaptiveActionPlanReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanReviewStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanReviewStatus.Created:
                switch (next)
                {
                    case AdaptiveActionPlanReviewStatus.Validated:
                        return true;
                }
                return false;
            case AdaptiveActionPlanReviewStatus.Validated:
                return next == AdaptiveActionPlanReviewStatus.Published || next == AdaptiveActionPlanReviewStatus.Retired;
            case AdaptiveActionPlanReviewStatus.Published:
                switch (next)
                {
                    case AdaptiveActionPlanReviewStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold + Score, 2);
    }

    public AdaptiveActionPlanReview Copy()
    {
        return new AdaptiveActionPlanReview(Code, Title, Channel, Location, Threshold, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
