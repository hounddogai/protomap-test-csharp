namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall quota is in its life cycle.</summary>
public enum BatchedTownHallQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched town hall quota as the town hall team keeps it.</summary>
public sealed class BatchedTownHallQuota(string code, string label, string reviewer, string grade, string managerId, int sequence, BatchedTownHallQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public BatchedTownHallQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall quota in status new.</summary>
    public static BatchedTownHallQuota NewBatchedTownHallQuota(string code, string label)
    {
        return new BatchedTownHallQuota(code, label, "", "", "", 0, BatchedTownHallQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallQuotaStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallQuotaStatus.New:
                return next == BatchedTownHallQuotaStatus.Queued || next == BatchedTownHallQuotaStatus.Running;
            case BatchedTownHallQuotaStatus.Queued:
                switch (next)
                {
                    case BatchedTownHallQuotaStatus.Failed:
                        return true;
                }
                return false;
            case BatchedTownHallQuotaStatus.Running:
                switch (next)
                {
                    case BatchedTownHallQuotaStatus.Done:
                    case BatchedTownHallQuotaStatus.Failed:
                        return true;
                }
                return false;
            case BatchedTownHallQuotaStatus.Done:
                switch (next)
                {
                    case BatchedTownHallQuotaStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public BatchedTownHallQuota Copy()
    {
        return new BatchedTownHallQuota(Code, Label, Reviewer, Grade, ManagerId, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
