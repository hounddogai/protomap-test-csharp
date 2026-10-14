namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional sentiment milestone is in its life cycle.</summary>
public enum RegionalSentimentMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional sentiment milestone. Its code is unique within a tenant.</summary>
public sealed class RegionalSentimentMilestone(string code, string label, int priority, string contactEmail, int attempts, RegionalSentimentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public int Attempts { get; set; } = attempts;
    public RegionalSentimentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a regional sentiment milestone in status new.</summary>
    public static RegionalSentimentMilestone NewRegionalSentimentMilestone(string code, string label)
    {
        return new RegionalSentimentMilestone(code, label, 0, "", 0, RegionalSentimentMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSentimentMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSentimentMilestoneStatus next)
    {
        switch (Status)
        {
            case RegionalSentimentMilestoneStatus.New:
                switch (next)
                {
                    case RegionalSentimentMilestoneStatus.Done:
                    case RegionalSentimentMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case RegionalSentimentMilestoneStatus.Queued:
                return next == RegionalSentimentMilestoneStatus.Running;
            case RegionalSentimentMilestoneStatus.Running:
                return next == RegionalSentimentMilestoneStatus.Done || next == RegionalSentimentMilestoneStatus.Failed;
            case RegionalSentimentMilestoneStatus.Done:
                switch (next)
                {
                    case RegionalSentimentMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSentimentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional sentiment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional sentiment milestone"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Priority * (double)Attempts, 2);
    }

    public RegionalSentimentMilestone Copy()
    {
        return new RegionalSentimentMilestone(Code, Label, Priority, ContactEmail, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
