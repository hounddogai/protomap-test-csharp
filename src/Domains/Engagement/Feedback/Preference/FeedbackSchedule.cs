namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback schedule is in its life cycle.</summary>
public enum FeedbackScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A feedback schedule as the feedback team keeps it.</summary>
public sealed class FeedbackSchedule(string code, string label, double maximum, Dictionary<string, string> attributes, string ownerId, double score, FeedbackScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public FeedbackScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a feedback schedule in status proposed.</summary>
    public static FeedbackSchedule NewFeedbackSchedule(string code, string label)
    {
        return new FeedbackSchedule(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            "",
            0.0,
            FeedbackScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackScheduleStatus next)
    {
        switch (Status)
        {
            case FeedbackScheduleStatus.Proposed:
                return next == FeedbackScheduleStatus.Approved;
            case FeedbackScheduleStatus.Approved:
                switch (next)
                {
                    case FeedbackScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case FeedbackScheduleStatus.Rejected:
                switch (next)
                {
                    case FeedbackScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double EffectiveTotal()
    {
        return Math.Round(Score + Maximum, 2);
    }

    public FeedbackSchedule Copy()
    {
        return new FeedbackSchedule(Code, Label, Maximum, Attributes, OwnerId, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
