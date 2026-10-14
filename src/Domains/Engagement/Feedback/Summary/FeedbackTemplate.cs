namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback template is in its life cycle.</summary>
public enum FeedbackTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A feedback template as the feedback team keeps it.</summary>
public sealed class FeedbackTemplate(string code, string name, string summary, double amount, int retries, int createdAt, List<string> tags, string notes, int durationMinutes, string externalRef, FeedbackTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public List<string> Tags { get; set; } = tags;
    public string Notes { get; set; } = notes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ExternalRef { get; set; } = externalRef;
    public FeedbackTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a feedback template in status proposed.</summary>
    public static FeedbackTemplate NewFeedbackTemplate(string code, string name)
    {
        return new FeedbackTemplate(
            code,
            name,
            "",
            0.0,
            0,
            0,
            new List<string>(),
            "",
            0,
            "",
            FeedbackTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackTemplateStatus next)
    {
        switch (Status)
        {
            case FeedbackTemplateStatus.Proposed:
                switch (next)
                {
                    case FeedbackTemplateStatus.Approved:
                        return true;
                }
                return false;
            case FeedbackTemplateStatus.Approved:
                return next == FeedbackTemplateStatus.Rejected || next == FeedbackTemplateStatus.Withdrawn;
            case FeedbackTemplateStatus.Rejected:
                switch (next)
                {
                    case FeedbackTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)CreatedAt + (double)DurationMinutes, 2);
    }

    public FeedbackTemplate Copy()
    {
        return new FeedbackTemplate(
            Code,
            Name,
            Summary,
            Amount,
            Retries,
            CreatedAt,
            Tags,
            Notes,
            DurationMinutes,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
