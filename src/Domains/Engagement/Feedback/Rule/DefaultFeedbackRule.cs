namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default feedback rule is in its life cycle.</summary>
public enum DefaultFeedbackRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default feedback rule, with what the planners need to act on it.</summary>
public sealed class DefaultFeedbackRule(string code, string label, string channel, string jobCode, int retries, int createdAt, DefaultFeedbackRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public DefaultFeedbackRuleStatus Status { get; set; } = status;

    /// <summary>Starts a default feedback rule in status new.</summary>
    public static DefaultFeedbackRule NewDefaultFeedbackRule(string code, string label)
    {
        return new DefaultFeedbackRule(code, label, "", "", 0, 0, DefaultFeedbackRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFeedbackRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFeedbackRuleStatus next)
    {
        switch (Status)
        {
            case DefaultFeedbackRuleStatus.New:
                switch (next)
                {
                    case DefaultFeedbackRuleStatus.Running:
                        return true;
                }
                return false;
            case DefaultFeedbackRuleStatus.Queued:
                switch (next)
                {
                    case DefaultFeedbackRuleStatus.Running:
                        return true;
                }
                return false;
            case DefaultFeedbackRuleStatus.Running:
                switch (next)
                {
                    case DefaultFeedbackRuleStatus.Failed:
                        return true;
                }
                return false;
            case DefaultFeedbackRuleStatus.Done:
                switch (next)
                {
                    case DefaultFeedbackRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultFeedbackRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default feedback rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt + (double)Retries, 2);
    }

    public DefaultFeedbackRule Copy()
    {
        return new DefaultFeedbackRule(Code, Label, Channel, JobCode, Retries, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
