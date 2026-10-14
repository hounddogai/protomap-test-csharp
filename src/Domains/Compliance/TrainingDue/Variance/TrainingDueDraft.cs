namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due draft is in its life cycle.</summary>
public enum TrainingDueDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A training due draft as the training due team keeps it.</summary>
public sealed class TrainingDueDraft(string code, string title, string managerId, double rating, string grade, List<string> tags, string summary, int batchSize, bool visible, TrainingDueDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public string Grade { get; set; } = grade;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public int BatchSize { get; set; } = batchSize;
    public bool Visible { get; set; } = visible;
    public TrainingDueDraftStatus Status { get; set; } = status;

    /// <summary>Starts a training due draft in status new.</summary>
    public static TrainingDueDraft NewTrainingDueDraft(string code, string title)
    {
        return new TrainingDueDraft(
            code,
            title,
            "",
            0.0,
            "",
            new List<string>(),
            "",
            0,
            false,
            TrainingDueDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueDraftStatus next)
    {
        switch (Status)
        {
            case TrainingDueDraftStatus.New:
                return next == TrainingDueDraftStatus.Running;
            case TrainingDueDraftStatus.Queued:
                return next == TrainingDueDraftStatus.Running;
            case TrainingDueDraftStatus.Running:
                return next == TrainingDueDraftStatus.Done || next == TrainingDueDraftStatus.Failed;
            case TrainingDueDraftStatus.Done:
                switch (next)
                {
                    case TrainingDueDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"training due draft"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize * Rating, 2);
    }

    public TrainingDueDraft Copy()
    {
        return new TrainingDueDraft(
            Code,
            Title,
            ManagerId,
            Rating,
            Grade,
            Tags,
            Summary,
            BatchSize,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
