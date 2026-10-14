namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback summary is in its life cycle.</summary>
public enum FeedbackSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A feedback summary as the feedback team keeps it.</summary>
public sealed class FeedbackSummary(string code, string label, Dictionary<string, string> attributes, int expiresAt, bool pinned, double margin, int capacity, string reviewer, int endsAt, string skill, int priority, FeedbackSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public double Margin { get; set; } = margin;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public FeedbackSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a feedback summary in status new.</summary>
    public static FeedbackSummary NewFeedbackSummary(string code, string label)
    {
        return new FeedbackSummary(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            false,
            0.0,
            0,
            "",
            0,
            "",
            0,
            FeedbackSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackSummaryStatus next)
    {
        switch (Status)
        {
            case FeedbackSummaryStatus.New:
                return next == FeedbackSummaryStatus.Running || next == FeedbackSummaryStatus.Done;
            case FeedbackSummaryStatus.Queued:
                switch (next)
                {
                    case FeedbackSummaryStatus.Done:
                        return true;
                }
                return false;
            case FeedbackSummaryStatus.Running:
                switch (next)
                {
                    case FeedbackSummaryStatus.Done:
                    case FeedbackSummaryStatus.Failed:
                        return true;
                }
                return false;
            case FeedbackSummaryStatus.Done:
                return next == FeedbackSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Priority - (double)ExpiresAt, 2);
    }

    public FeedbackSummary Copy()
    {
        return new FeedbackSummary(
            Code,
            Label,
            Attributes,
            ExpiresAt,
            Pinned,
            Margin,
            Capacity,
            Reviewer,
            EndsAt,
            Skill,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
