namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted training due cycle is in its life cycle.</summary>
public enum WeightedTrainingDueCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted training due cycle as the training due team keeps it.</summary>
public sealed class WeightedTrainingDueCycle(string code, string label, string skill, int updatedAt, int batchSize, int revision, int quantity, string managerId, int sequence, WeightedTrainingDueCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public int Revision { get; set; } = revision;
    public int Quantity { get; set; } = quantity;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public WeightedTrainingDueCycleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted training due cycle in status planned.</summary>
    public static WeightedTrainingDueCycle NewWeightedTrainingDueCycle(string code, string label)
    {
        return new WeightedTrainingDueCycle(
            code,
            label,
            "",
            0,
            0,
            0,
            0,
            "",
            0,
            WeightedTrainingDueCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTrainingDueCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTrainingDueCycleStatus next)
    {
        switch (Status)
        {
            case WeightedTrainingDueCycleStatus.Planned:
                switch (next)
                {
                    case WeightedTrainingDueCycleStatus.Scheduled:
                        return true;
                }
                return false;
            case WeightedTrainingDueCycleStatus.Scheduled:
                return next == WeightedTrainingDueCycleStatus.Cancelled;
            case WeightedTrainingDueCycleStatus.InProgress:
                switch (next)
                {
                    case WeightedTrainingDueCycleStatus.Completed:
                    case WeightedTrainingDueCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedTrainingDueCycleStatus.Completed:
                switch (next)
                {
                    case WeightedTrainingDueCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedTrainingDueCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted training due cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)UpdatedAt - (double)Revision, 2);
    }

    public WeightedTrainingDueCycle Copy()
    {
        return new WeightedTrainingDueCycle(
            Code,
            Label,
            Skill,
            UpdatedAt,
            BatchSize,
            Revision,
            Quantity,
            ManagerId,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
