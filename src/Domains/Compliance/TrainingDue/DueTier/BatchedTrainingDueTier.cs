namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched training due tier is in its life cycle.</summary>
public enum BatchedTrainingDueTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched training due tier as the training due team keeps it.</summary>
public sealed class BatchedTrainingDueTier(string code, string label, bool archived, string title, int leaveDays, int endsAt, BatchedTrainingDueTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int EndsAt { get; set; } = endsAt;
    public BatchedTrainingDueTierStatus Status { get; set; } = status;

    /// <summary>Starts a batched training due tier in status new.</summary>
    public static BatchedTrainingDueTier NewBatchedTrainingDueTier(string code, string label)
    {
        return new BatchedTrainingDueTier(code, label, false, "", 0, 0, BatchedTrainingDueTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTrainingDueTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTrainingDueTierStatus next)
    {
        switch (Status)
        {
            case BatchedTrainingDueTierStatus.New:
                return next == BatchedTrainingDueTierStatus.Queued || next == BatchedTrainingDueTierStatus.Failed;
            case BatchedTrainingDueTierStatus.Queued:
                return next == BatchedTrainingDueTierStatus.Done || next == BatchedTrainingDueTierStatus.Failed;
            case BatchedTrainingDueTierStatus.Running:
                switch (next)
                {
                    case BatchedTrainingDueTierStatus.Done:
                    case BatchedTrainingDueTierStatus.Failed:
                        return true;
                }
                return false;
            case BatchedTrainingDueTierStatus.Done:
                return next == BatchedTrainingDueTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedTrainingDueTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched training due tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)LeaveDays - (double)EndsAt, 2);
    }

    public BatchedTrainingDueTier Copy()
    {
        return new BatchedTrainingDueTier(Code, Label, Archived, Title, LeaveDays, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
