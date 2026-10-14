namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive feedback score is in its life cycle.</summary>
public enum AdaptiveFeedbackScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adaptive feedback score and its current status.</summary>
public sealed class AdaptiveFeedbackScore(string code, string title, bool archived, double budget, string badge, List<string> tags, string costCenter, string country, string department, AdaptiveFeedbackScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public string Department { get; set; } = department;
    public AdaptiveFeedbackScoreStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive feedback score in status new.</summary>
    public static AdaptiveFeedbackScore NewAdaptiveFeedbackScore(string code, string title)
    {
        return new AdaptiveFeedbackScore(
            code,
            title,
            false,
            0.0,
            "",
            new List<string>(),
            "",
            "",
            "",
            AdaptiveFeedbackScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFeedbackScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFeedbackScoreStatus next)
    {
        switch (Status)
        {
            case AdaptiveFeedbackScoreStatus.New:
                return next == AdaptiveFeedbackScoreStatus.Failed;
            case AdaptiveFeedbackScoreStatus.Queued:
                return next == AdaptiveFeedbackScoreStatus.Running;
            case AdaptiveFeedbackScoreStatus.Running:
                switch (next)
                {
                    case AdaptiveFeedbackScoreStatus.Done:
                    case AdaptiveFeedbackScoreStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveFeedbackScoreStatus.Done:
                switch (next)
                {
                    case AdaptiveFeedbackScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveFeedbackScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive feedback score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public AdaptiveFeedbackScore Copy()
    {
        return new AdaptiveFeedbackScore(
            Code,
            Title,
            Archived,
            Budget,
            Badge,
            Tags,
            CostCenter,
            Country,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
