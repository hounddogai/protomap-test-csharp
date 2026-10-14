namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback manifest is in its life cycle.</summary>
public enum FeedbackManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a feedback manifest and its current status.</summary>
public sealed class FeedbackManifest(string code, string name, double cost, string skill, string grade, bool approved, string description, FeedbackManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public FeedbackManifestStatus Status { get; set; } = status;

    /// <summary>Starts a feedback manifest in status planned.</summary>
    public static FeedbackManifest NewFeedbackManifest(string code, string name)
    {
        return new FeedbackManifest(code, name, 0.0, "", "", false, "", FeedbackManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackManifestStatus next)
    {
        switch (Status)
        {
            case FeedbackManifestStatus.Planned:
                return next == FeedbackManifestStatus.Completed || next == FeedbackManifestStatus.Cancelled;
            case FeedbackManifestStatus.Scheduled:
                return next == FeedbackManifestStatus.InProgress;
            case FeedbackManifestStatus.InProgress:
                return next == FeedbackManifestStatus.Cancelled;
            case FeedbackManifestStatus.Completed:
                return next == FeedbackManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FeedbackManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public FeedbackManifest Copy()
    {
        return new FeedbackManifest(Code, Name, Cost, Skill, Grade, Approved, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
