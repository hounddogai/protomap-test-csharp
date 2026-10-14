namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched accrual checkpoint is in its life cycle.</summary>
public enum BatchedAccrualCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One batched accrual checkpoint, with what the planners need to act on it.</summary>
public sealed class BatchedAccrualCheckpoint(string code, string label, string source, double threshold, string skill, string locale, double floorValue, string team, double margin, BatchedAccrualCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public string Skill { get; set; } = skill;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public double Margin { get; set; } = margin;
    public BatchedAccrualCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a batched accrual checkpoint in status idle.</summary>
    public static BatchedAccrualCheckpoint NewBatchedAccrualCheckpoint(string code, string label)
    {
        return new BatchedAccrualCheckpoint(
            code,
            label,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            0.0,
            BatchedAccrualCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAccrualCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAccrualCheckpointStatus next)
    {
        switch (Status)
        {
            case BatchedAccrualCheckpointStatus.Idle:
                return next == BatchedAccrualCheckpointStatus.Stopped;
            case BatchedAccrualCheckpointStatus.Warming:
                return next == BatchedAccrualCheckpointStatus.Ready || next == BatchedAccrualCheckpointStatus.Stopped;
            case BatchedAccrualCheckpointStatus.Ready:
                switch (next)
                {
                    case BatchedAccrualCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case BatchedAccrualCheckpointStatus.Draining:
                switch (next)
                {
                    case BatchedAccrualCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAccrualCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched accrual checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched accrual checkpoint"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Margin - Threshold, 2);
    }

    public BatchedAccrualCheckpoint Copy()
    {
        return new BatchedAccrualCheckpoint(
            Code,
            Label,
            Source,
            Threshold,
            Skill,
            Locale,
            FloorValue,
            Team,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
