namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due mapping is in its life cycle.</summary>
public enum TrainingDueMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a training due mapping and its current status.</summary>
public sealed class TrainingDueMapping(string code, string name, List<string> tags, string segment, string badge, double score, TrainingDueMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Segment { get; set; } = segment;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public TrainingDueMappingStatus Status { get; set; } = status;

    /// <summary>Starts a training due mapping in status planned.</summary>
    public static TrainingDueMapping NewTrainingDueMapping(string code, string name)
    {
        return new TrainingDueMapping(code, name, new List<string>(), "", "", 0.0, TrainingDueMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueMappingStatus next)
    {
        switch (Status)
        {
            case TrainingDueMappingStatus.Planned:
                return next == TrainingDueMappingStatus.InProgress;
            case TrainingDueMappingStatus.Scheduled:
                return next == TrainingDueMappingStatus.InProgress || next == TrainingDueMappingStatus.Cancelled;
            case TrainingDueMappingStatus.InProgress:
                return next == TrainingDueMappingStatus.Completed || next == TrainingDueMappingStatus.Cancelled;
            case TrainingDueMappingStatus.Completed:
                switch (next)
                {
                    case TrainingDueMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public TrainingDueMapping Copy()
    {
        return new TrainingDueMapping(Code, Name, Tags, Segment, Badge, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
