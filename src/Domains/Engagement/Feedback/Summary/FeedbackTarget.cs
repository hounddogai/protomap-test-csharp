namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback target is in its life cycle.</summary>
public enum FeedbackTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A feedback target as the feedback team keeps it.</summary>
public sealed class FeedbackTarget(string code, string title, string summary, int endsAt, int createdAt, string country, string contactEmail, FeedbackTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public FeedbackTargetStatus Status { get; set; } = status;

    /// <summary>Starts a feedback target in status new.</summary>
    public static FeedbackTarget NewFeedbackTarget(string code, string title)
    {
        return new FeedbackTarget(code, title, "", 0, 0, "", "", FeedbackTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackTargetStatus next)
    {
        switch (Status)
        {
            case FeedbackTargetStatus.New:
                switch (next)
                {
                    case FeedbackTargetStatus.Queued:
                        return true;
                }
                return false;
            case FeedbackTargetStatus.Queued:
                return next == FeedbackTargetStatus.Running || next == FeedbackTargetStatus.Done;
            case FeedbackTargetStatus.Running:
                switch (next)
                {
                    case FeedbackTargetStatus.Done:
                        return true;
                }
                return false;
            case FeedbackTargetStatus.Done:
                return next == FeedbackTargetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt + (double)CreatedAt, 2);
    }

    public FeedbackTarget Copy()
    {
        return new FeedbackTarget(Code, Title, Summary, EndsAt, CreatedAt, Country, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
