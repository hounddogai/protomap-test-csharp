namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due checkpoint is in its life cycle.</summary>
public enum TrainingDueCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A training due checkpoint as the training due team keeps it.</summary>
public sealed class TrainingDueCheckpoint(string code, string title, int sequence, Dictionary<string, string> attributes, string segment, int priority, int limit, string reviewer, int expiresAt, TrainingDueCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public TrainingDueCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a training due checkpoint in status idle.</summary>
    public static TrainingDueCheckpoint NewTrainingDueCheckpoint(string code, string title)
    {
        return new TrainingDueCheckpoint(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            "",
            0,
            0,
            "",
            0,
            TrainingDueCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueCheckpointStatus next)
    {
        switch (Status)
        {
            case TrainingDueCheckpointStatus.Idle:
                switch (next)
                {
                    case TrainingDueCheckpointStatus.Ready:
                        return true;
                }
                return false;
            case TrainingDueCheckpointStatus.Warming:
                return next == TrainingDueCheckpointStatus.Ready;
            case TrainingDueCheckpointStatus.Ready:
                switch (next)
                {
                    case TrainingDueCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case TrainingDueCheckpointStatus.Draining:
                switch (next)
                {
                    case TrainingDueCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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
        return Math.Round((double)Priority + (double)Sequence, 2);
    }

    public TrainingDueCheckpoint Copy()
    {
        return new TrainingDueCheckpoint(
            Code,
            Title,
            Sequence,
            Attributes,
            Segment,
            Priority,
            Limit,
            Reviewer,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
