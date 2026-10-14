namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched training due mapping is in its life cycle.</summary>
public enum BatchedTrainingDueMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One batched training due mapping, with what the planners need to act on it.</summary>
public sealed class BatchedTrainingDueMapping(string code, string title, int revision, double hours, double maximum, string unit, string costCenter, BatchedTrainingDueMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public BatchedTrainingDueMappingStatus Status { get; set; } = status;

    /// <summary>Starts a batched training due mapping in status idle.</summary>
    public static BatchedTrainingDueMapping NewBatchedTrainingDueMapping(string code, string title)
    {
        return new BatchedTrainingDueMapping(code, title, 0, 0.0, 0.0, "", "", BatchedTrainingDueMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTrainingDueMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTrainingDueMappingStatus next)
    {
        switch (Status)
        {
            case BatchedTrainingDueMappingStatus.Idle:
                switch (next)
                {
                    case BatchedTrainingDueMappingStatus.Draining:
                        return true;
                }
                return false;
            case BatchedTrainingDueMappingStatus.Warming:
                switch (next)
                {
                    case BatchedTrainingDueMappingStatus.Draining:
                        return true;
                }
                return false;
            case BatchedTrainingDueMappingStatus.Ready:
                return next == BatchedTrainingDueMappingStatus.Stopped;
            case BatchedTrainingDueMappingStatus.Draining:
                switch (next)
                {
                    case BatchedTrainingDueMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTrainingDueMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched training due mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Maximum - Hours, 2);
    }

    public BatchedTrainingDueMapping Copy()
    {
        return new BatchedTrainingDueMapping(Code, Title, Revision, Hours, Maximum, Unit, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
