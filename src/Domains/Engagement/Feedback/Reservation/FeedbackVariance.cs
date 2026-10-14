namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback variance is in its life cycle.</summary>
public enum FeedbackVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a feedback variance and its current status.</summary>
public sealed class FeedbackVariance(string code, string title, bool approved, List<string> tags, double cost, string locale, string region, string salaryBand, FeedbackVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public string SalaryBand { get; set; } = salaryBand;
    public FeedbackVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a feedback variance in status new.</summary>
    public static FeedbackVariance NewFeedbackVariance(string code, string title)
    {
        return new FeedbackVariance(
            code,
            title,
            false,
            new List<string>(),
            0.0,
            "",
            "",
            "",
            FeedbackVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackVarianceStatus next)
    {
        switch (Status)
        {
            case FeedbackVarianceStatus.New:
                return next == FeedbackVarianceStatus.Done || next == FeedbackVarianceStatus.Failed;
            case FeedbackVarianceStatus.Queued:
                return next == FeedbackVarianceStatus.Failed;
            case FeedbackVarianceStatus.Running:
                return next == FeedbackVarianceStatus.Failed;
            case FeedbackVarianceStatus.Done:
                return next == FeedbackVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback variance"} {Code}";
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
        return Cost;
    }

    public FeedbackVariance Copy()
    {
        return new FeedbackVariance(Code, Title, Approved, Tags, Cost, Locale, Region, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
