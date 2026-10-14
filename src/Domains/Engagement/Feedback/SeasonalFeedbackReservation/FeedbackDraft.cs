namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback draft is in its life cycle.</summary>
public enum FeedbackDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One feedback draft, with what the planners need to act on it.</summary>
public sealed class FeedbackDraft(string code, string name, string phone, double threshold, string notes, string grade, int revision, string label, string description, int windowDays, FeedbackDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Threshold { get; set; } = threshold;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public int Revision { get; set; } = revision;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public FeedbackDraftStatus Status { get; set; } = status;

    /// <summary>Starts a feedback draft in status created.</summary>
    public static FeedbackDraft NewFeedbackDraft(string code, string name)
    {
        return new FeedbackDraft(code, name, "", 0.0, "", "", 0, "", "", 0, FeedbackDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackDraftStatus next)
    {
        switch (Status)
        {
            case FeedbackDraftStatus.Created:
                switch (next)
                {
                    case FeedbackDraftStatus.Validated:
                    case FeedbackDraftStatus.Retired:
                        return true;
                }
                return false;
            case FeedbackDraftStatus.Validated:
                switch (next)
                {
                    case FeedbackDraftStatus.Published:
                    case FeedbackDraftStatus.Retired:
                        return true;
                }
                return false;
            case FeedbackDraftStatus.Published:
                switch (next)
                {
                    case FeedbackDraftStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public FeedbackDraft Copy()
    {
        return new FeedbackDraft(
            Code,
            Name,
            Phone,
            Threshold,
            Notes,
            Grade,
            Revision,
            Label,
            Description,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
