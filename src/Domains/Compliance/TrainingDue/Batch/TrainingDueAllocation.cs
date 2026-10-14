namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due allocation is in its life cycle.</summary>
public enum TrainingDueAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a training due allocation and its current status.</summary>
public sealed class TrainingDueAllocation(string code, string title, string notes, Dictionary<string, string> attributes, bool visible, string checksum, int limit, bool archived, double fte, string location, TrainingDueAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Visible { get; set; } = visible;
    public string Checksum { get; set; } = checksum;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public TrainingDueAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a training due allocation in status new.</summary>
    public static TrainingDueAllocation NewTrainingDueAllocation(string code, string title)
    {
        return new TrainingDueAllocation(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            false,
            "",
            0,
            false,
            0.0,
            "",
            TrainingDueAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueAllocationStatus next)
    {
        switch (Status)
        {
            case TrainingDueAllocationStatus.New:
                return next == TrainingDueAllocationStatus.Queued || next == TrainingDueAllocationStatus.Running;
            case TrainingDueAllocationStatus.Queued:
                return next == TrainingDueAllocationStatus.Done || next == TrainingDueAllocationStatus.Failed;
            case TrainingDueAllocationStatus.Running:
                return next == TrainingDueAllocationStatus.Failed;
            case TrainingDueAllocationStatus.Done:
                switch (next)
                {
                    case TrainingDueAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"training due allocation"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Fte - (double)Limit, 2);
    }

    public TrainingDueAllocation Copy()
    {
        return new TrainingDueAllocation(
            Code,
            Title,
            Notes,
            Attributes,
            Visible,
            Checksum,
            Limit,
            Archived,
            Fte,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
