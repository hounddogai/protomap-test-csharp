namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched reorg snapshot is in its life cycle.</summary>
public enum BatchedReorgSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One batched reorg snapshot, with what the planners need to act on it.</summary>
public sealed class BatchedReorgSnapshot(string code, string name, string phone, string salaryBand, int startsAt, double score, string costCenter, double discount, BatchedReorgSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string SalaryBand { get; set; } = salaryBand;
    public int StartsAt { get; set; } = startsAt;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public BatchedReorgSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched reorg snapshot in status idle.</summary>
    public static BatchedReorgSnapshot NewBatchedReorgSnapshot(string code, string name)
    {
        return new BatchedReorgSnapshot(code, name, "", "", 0, 0.0, "", 0.0, BatchedReorgSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedReorgSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedReorgSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedReorgSnapshotStatus.Idle:
                switch (next)
                {
                    case BatchedReorgSnapshotStatus.Draining:
                    case BatchedReorgSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedReorgSnapshotStatus.Warming:
                switch (next)
                {
                    case BatchedReorgSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedReorgSnapshotStatus.Ready:
                switch (next)
                {
                    case BatchedReorgSnapshotStatus.Draining:
                    case BatchedReorgSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedReorgSnapshotStatus.Draining:
                return next == BatchedReorgSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedReorgSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched reorg snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched reorg snapshot"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Discount * (double)StartsAt, 2);
    }

    public BatchedReorgSnapshot Copy()
    {
        return new BatchedReorgSnapshot(
            Code,
            Name,
            Phone,
            SalaryBand,
            StartsAt,
            Score,
            CostCenter,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
