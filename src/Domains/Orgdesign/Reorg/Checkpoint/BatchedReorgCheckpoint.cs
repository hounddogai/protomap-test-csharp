namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched reorg checkpoint is in its life cycle.</summary>
public enum BatchedReorgCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched reorg checkpoint. Its code is unique within a tenant.</summary>
public sealed class BatchedReorgCheckpoint(string code, string title, string salaryBand, int durationMinutes, string segment, double floorValue, string channel, string jobCode, BatchedReorgCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public BatchedReorgCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a batched reorg checkpoint in status requested.</summary>
    public static BatchedReorgCheckpoint NewBatchedReorgCheckpoint(string code, string title)
    {
        return new BatchedReorgCheckpoint(
            code,
            title,
            "",
            0,
            "",
            0.0,
            "",
            "",
            BatchedReorgCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedReorgCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedReorgCheckpointStatus next)
    {
        switch (Status)
        {
            case BatchedReorgCheckpointStatus.Requested:
                switch (next)
                {
                    case BatchedReorgCheckpointStatus.Revoked:
                    case BatchedReorgCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case BatchedReorgCheckpointStatus.Granted:
                switch (next)
                {
                    case BatchedReorgCheckpointStatus.Revoked:
                    case BatchedReorgCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case BatchedReorgCheckpointStatus.Revoked:
                return next == BatchedReorgCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedReorgCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched reorg checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched reorg checkpoint"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue + (double)DurationMinutes, 2);
    }

    public BatchedReorgCheckpoint Copy()
    {
        return new BatchedReorgCheckpoint(
            Code,
            Title,
            SalaryBand,
            DurationMinutes,
            Segment,
            FloorValue,
            Channel,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
