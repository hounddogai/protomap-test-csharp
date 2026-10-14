namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched recognition draft is in its life cycle.</summary>
public enum BatchedRecognitionDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched recognition draft as the recognition team keeps it.</summary>
public sealed class BatchedRecognitionDraft(string code, string name, double floorValue, string skill, double budget, string phone, string checksum, string location, double weight, string badge, BatchedRecognitionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public double Budget { get; set; } = budget;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public BatchedRecognitionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a batched recognition draft in status planned.</summary>
    public static BatchedRecognitionDraft NewBatchedRecognitionDraft(string code, string name)
    {
        return new BatchedRecognitionDraft(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            BatchedRecognitionDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRecognitionDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRecognitionDraftStatus next)
    {
        switch (Status)
        {
            case BatchedRecognitionDraftStatus.Planned:
                return next == BatchedRecognitionDraftStatus.Scheduled || next == BatchedRecognitionDraftStatus.InProgress;
            case BatchedRecognitionDraftStatus.Scheduled:
                return next == BatchedRecognitionDraftStatus.Completed || next == BatchedRecognitionDraftStatus.Cancelled;
            case BatchedRecognitionDraftStatus.InProgress:
                switch (next)
                {
                    case BatchedRecognitionDraftStatus.Completed:
                    case BatchedRecognitionDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedRecognitionDraftStatus.Completed:
                return next == BatchedRecognitionDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedRecognitionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched recognition draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched recognition draft"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(FloorValue - Weight, 2);
    }

    public BatchedRecognitionDraft Copy()
    {
        return new BatchedRecognitionDraft(
            Code,
            Name,
            FloorValue,
            Skill,
            Budget,
            Phone,
            Checksum,
            Location,
            Weight,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
