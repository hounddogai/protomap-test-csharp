namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback feedback entry is in its life cycle.</summary>
public enum FallbackFeedbackEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One fallback feedback entry, with what the planners need to act on it.</summary>
public sealed class FallbackFeedbackEntry(string code, string label, int leaveDays, string team, string phone, string channel, string region, FallbackFeedbackEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public FallbackFeedbackEntryStatus Status { get; set; } = status;

    /// <summary>Starts a fallback feedback entry in status open.</summary>
    public static FallbackFeedbackEntry NewFallbackFeedbackEntry(string code, string label)
    {
        return new FallbackFeedbackEntry(code, label, 0, "", "", "", "", FallbackFeedbackEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackFeedbackEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackFeedbackEntryStatus next)
    {
        switch (Status)
        {
            case FallbackFeedbackEntryStatus.Open:
                return next == FallbackFeedbackEntryStatus.Resolved || next == FallbackFeedbackEntryStatus.Reopened;
            case FallbackFeedbackEntryStatus.InReview:
                switch (next)
                {
                    case FallbackFeedbackEntryStatus.Resolved:
                    case FallbackFeedbackEntryStatus.Archived:
                        return true;
                }
                return false;
            case FallbackFeedbackEntryStatus.Resolved:
                switch (next)
                {
                    case FallbackFeedbackEntryStatus.Reopened:
                    case FallbackFeedbackEntryStatus.Archived:
                        return true;
                }
                return false;
            case FallbackFeedbackEntryStatus.Reopened:
                switch (next)
                {
                    case FallbackFeedbackEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackFeedbackEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback feedback entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback feedback entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public FallbackFeedbackEntry Copy()
    {
        return new FallbackFeedbackEntry(Code, Label, LeaveDays, Team, Phone, Channel, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
