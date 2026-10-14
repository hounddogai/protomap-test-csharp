namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay checkpoint is in its life cycle.</summary>
public enum RetroPayCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A retro pay checkpoint. Its code is unique within a tenant.</summary>
public sealed class RetroPayCheckpoint(string code, string label, string department, string employeeId, int revision, int version, int tenureMonths, double score, int attempts, int startsAt, int batchSize, RetroPayCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public int Revision { get; set; } = revision;
    public int Version { get; set; } = version;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public int StartsAt { get; set; } = startsAt;
    public int BatchSize { get; set; } = batchSize;
    public RetroPayCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay checkpoint in status proposed.</summary>
    public static RetroPayCheckpoint NewRetroPayCheckpoint(string code, string label)
    {
        return new RetroPayCheckpoint(code, label, "", "", 0, 0, 0, 0.0, 0, 0, 0, RetroPayCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayCheckpointStatus next)
    {
        switch (Status)
        {
            case RetroPayCheckpointStatus.Proposed:
                switch (next)
                {
                    case RetroPayCheckpointStatus.Approved:
                    case RetroPayCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case RetroPayCheckpointStatus.Approved:
                switch (next)
                {
                    case RetroPayCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case RetroPayCheckpointStatus.Rejected:
                return next == RetroPayCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RetroPayCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay checkpoint"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts + Score, 2);
    }

    public RetroPayCheckpoint Copy()
    {
        return new RetroPayCheckpoint(
            Code,
            Label,
            Department,
            EmployeeId,
            Revision,
            Version,
            TenureMonths,
            Score,
            Attempts,
            StartsAt,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
