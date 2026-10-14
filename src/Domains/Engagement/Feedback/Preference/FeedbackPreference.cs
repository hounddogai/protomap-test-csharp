namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback preference is in its life cycle.</summary>
public enum FeedbackPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One feedback preference, with what the planners need to act on it.</summary>
public sealed class FeedbackPreference(string code, string title, string managerId, string source, string ownerId, double margin, FeedbackPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public double Margin { get; set; } = margin;
    public FeedbackPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a feedback preference in status idle.</summary>
    public static FeedbackPreference NewFeedbackPreference(string code, string title)
    {
        return new FeedbackPreference(code, title, "", "", "", 0.0, FeedbackPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackPreferenceStatus next)
    {
        switch (Status)
        {
            case FeedbackPreferenceStatus.Idle:
                switch (next)
                {
                    case FeedbackPreferenceStatus.Warming:
                    case FeedbackPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case FeedbackPreferenceStatus.Warming:
                switch (next)
                {
                    case FeedbackPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case FeedbackPreferenceStatus.Ready:
                switch (next)
                {
                    case FeedbackPreferenceStatus.Draining:
                    case FeedbackPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackPreferenceStatus.Draining:
                return next == FeedbackPreferenceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FeedbackPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public FeedbackPreference Copy()
    {
        return new FeedbackPreference(Code, Title, ManagerId, Source, OwnerId, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
