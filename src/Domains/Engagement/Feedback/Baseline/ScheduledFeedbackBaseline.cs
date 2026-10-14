namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled feedback baseline is in its life cycle.</summary>
public enum ScheduledFeedbackBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled feedback baseline as the feedback team keeps it.</summary>
public sealed class ScheduledFeedbackBaseline(string code, string label, string externalRef, double ceiling, double score, string currency, ScheduledFeedbackBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public ScheduledFeedbackBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled feedback baseline in status open.</summary>
    public static ScheduledFeedbackBaseline NewScheduledFeedbackBaseline(string code, string label)
    {
        return new ScheduledFeedbackBaseline(code, label, "", 0.0, 0.0, "", ScheduledFeedbackBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFeedbackBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFeedbackBaselineStatus next)
    {
        switch (Status)
        {
            case ScheduledFeedbackBaselineStatus.Open:
                switch (next)
                {
                    case ScheduledFeedbackBaselineStatus.InReview:
                        return true;
                }
                return false;
            case ScheduledFeedbackBaselineStatus.InReview:
                switch (next)
                {
                    case ScheduledFeedbackBaselineStatus.Reopened:
                    case ScheduledFeedbackBaselineStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledFeedbackBaselineStatus.Resolved:
                return next == ScheduledFeedbackBaselineStatus.Reopened || next == ScheduledFeedbackBaselineStatus.Archived;
            case ScheduledFeedbackBaselineStatus.Reopened:
                return next == ScheduledFeedbackBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledFeedbackBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled feedback baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Score + Ceiling, 2);
    }

    public ScheduledFeedbackBaseline Copy()
    {
        return new ScheduledFeedbackBaseline(Code, Label, ExternalRef, Ceiling, Score, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
