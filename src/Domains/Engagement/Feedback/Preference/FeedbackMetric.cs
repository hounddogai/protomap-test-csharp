namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback metric is in its life cycle.</summary>
public enum FeedbackMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a feedback metric and its current status.</summary>
public sealed class FeedbackMetric(string code, string title, string name, string category, string summary, int quantity, FeedbackMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Summary { get; set; } = summary;
    public int Quantity { get; set; } = quantity;
    public FeedbackMetricStatus Status { get; set; } = status;

    /// <summary>Starts a feedback metric in status proposed.</summary>
    public static FeedbackMetric NewFeedbackMetric(string code, string title)
    {
        return new FeedbackMetric(code, title, "", "", "", 0, FeedbackMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackMetricStatus next)
    {
        switch (Status)
        {
            case FeedbackMetricStatus.Proposed:
                return next == FeedbackMetricStatus.Rejected;
            case FeedbackMetricStatus.Approved:
                return next == FeedbackMetricStatus.Rejected || next == FeedbackMetricStatus.Withdrawn;
            case FeedbackMetricStatus.Rejected:
                switch (next)
                {
                    case FeedbackMetricStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public FeedbackMetric Copy()
    {
        return new FeedbackMetric(Code, Title, Name, Category, Summary, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
