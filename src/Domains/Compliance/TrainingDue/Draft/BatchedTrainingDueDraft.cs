namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched training due draft is in its life cycle.</summary>
public enum BatchedTrainingDueDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched training due draft. Its code is unique within a tenant.</summary>
public sealed class BatchedTrainingDueDraft(string code, string title, double ratio, double floorValue, string skill, string badge, string location, string checksum, Dictionary<string, string> attributes, string department, BatchedTrainingDueDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public string Badge { get; set; } = badge;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public BatchedTrainingDueDraftStatus Status { get; set; } = status;

    /// <summary>Starts a batched training due draft in status new.</summary>
    public static BatchedTrainingDueDraft NewBatchedTrainingDueDraft(string code, string title)
    {
        return new BatchedTrainingDueDraft(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            BatchedTrainingDueDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTrainingDueDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTrainingDueDraftStatus next)
    {
        switch (Status)
        {
            case BatchedTrainingDueDraftStatus.New:
                return next == BatchedTrainingDueDraftStatus.Done;
            case BatchedTrainingDueDraftStatus.Queued:
                switch (next)
                {
                    case BatchedTrainingDueDraftStatus.Running:
                        return true;
                }
                return false;
            case BatchedTrainingDueDraftStatus.Running:
                return next == BatchedTrainingDueDraftStatus.Failed;
            case BatchedTrainingDueDraftStatus.Done:
                switch (next)
                {
                    case BatchedTrainingDueDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTrainingDueDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched training due draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double ProjectedTotal()
    {
        return Math.Round(FloorValue * Ratio, 2);
    }

    public BatchedTrainingDueDraft Copy()
    {
        return new BatchedTrainingDueDraft(
            Code,
            Title,
            Ratio,
            FloorValue,
            Skill,
            Badge,
            Location,
            Checksum,
            Attributes,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
