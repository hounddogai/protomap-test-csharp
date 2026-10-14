namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment plan is in its life cycle.</summary>
public enum SentimentPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a sentiment plan and its current status.</summary>
public sealed class SentimentPlan(string code, string label, double score, string category, double fte, double threshold, string ownerId, int attempts, double hours, double minimum, bool active, SentimentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public double Threshold { get; set; } = threshold;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public SentimentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment plan in status idle.</summary>
    public static SentimentPlan NewSentimentPlan(string code, string label)
    {
        return new SentimentPlan(code, label, 0.0, "", 0.0, 0.0, "", 0, 0.0, 0.0, false, SentimentPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentPlanStatus next)
    {
        switch (Status)
        {
            case SentimentPlanStatus.Idle:
                switch (next)
                {
                    case SentimentPlanStatus.Warming:
                        return true;
                }
                return false;
            case SentimentPlanStatus.Warming:
                return next == SentimentPlanStatus.Draining || next == SentimentPlanStatus.Stopped;
            case SentimentPlanStatus.Ready:
                switch (next)
                {
                    case SentimentPlanStatus.Draining:
                    case SentimentPlanStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentPlanStatus.Draining:
                return next == SentimentPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SentimentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"sentiment plan"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Minimum + (double)Attempts, 2);
    }

    public SentimentPlan Copy()
    {
        return new SentimentPlan(
            Code,
            Label,
            Score,
            Category,
            Fte,
            Threshold,
            OwnerId,
            Attempts,
            Hours,
            Minimum,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
