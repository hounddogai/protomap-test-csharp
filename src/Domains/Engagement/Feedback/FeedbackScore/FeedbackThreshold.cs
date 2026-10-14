namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback threshold is in its life cycle.</summary>
public enum FeedbackThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a feedback threshold and its current status.</summary>
public sealed class FeedbackThreshold(string code, string label, string channel, string costCenter, string badge, int priority, bool pinned, string unit, int capacity, string locale, FeedbackThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public int Priority { get; set; } = priority;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public FeedbackThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a feedback threshold in status draft.</summary>
    public static FeedbackThreshold NewFeedbackThreshold(string code, string label)
    {
        return new FeedbackThreshold(code, label, "", "", "", 0, false, "", 0, "", FeedbackThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackThresholdStatus next)
    {
        switch (Status)
        {
            case FeedbackThresholdStatus.Draft:
                return next == FeedbackThresholdStatus.Pending;
            case FeedbackThresholdStatus.Pending:
                return next == FeedbackThresholdStatus.Suspended || next == FeedbackThresholdStatus.Closed;
            case FeedbackThresholdStatus.Active:
                switch (next)
                {
                    case FeedbackThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackThresholdStatus.Suspended:
                switch (next)
                {
                    case FeedbackThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public FeedbackThreshold Copy()
    {
        return new FeedbackThreshold(
            Code,
            Label,
            Channel,
            CostCenter,
            Badge,
            Priority,
            Pinned,
            Unit,
            Capacity,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
