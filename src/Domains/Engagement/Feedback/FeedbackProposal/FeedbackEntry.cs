namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback entry is in its life cycle.</summary>
public enum FeedbackEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One feedback entry, with what the planners need to act on it.</summary>
public sealed class FeedbackEntry(string code, string label, bool visible, int windowDays, int durationMinutes, FeedbackEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int WindowDays { get; set; } = windowDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public FeedbackEntryStatus Status { get; set; } = status;

    /// <summary>Starts a feedback entry in status draft.</summary>
    public static FeedbackEntry NewFeedbackEntry(string code, string label)
    {
        return new FeedbackEntry(code, label, false, 0, 0, FeedbackEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackEntryStatus next)
    {
        switch (Status)
        {
            case FeedbackEntryStatus.Draft:
                switch (next)
                {
                    case FeedbackEntryStatus.Active:
                        return true;
                }
                return false;
            case FeedbackEntryStatus.Pending:
                switch (next)
                {
                    case FeedbackEntryStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackEntryStatus.Active:
                switch (next)
                {
                    case FeedbackEntryStatus.Closed:
                        return true;
                }
                return false;
            case FeedbackEntryStatus.Suspended:
                return next == FeedbackEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays - (double)DurationMinutes, 2);
    }

    public FeedbackEntry Copy()
    {
        return new FeedbackEntry(Code, Label, Visible, WindowDays, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
