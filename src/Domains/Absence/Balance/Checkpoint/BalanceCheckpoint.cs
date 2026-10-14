namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance checkpoint is in its life cycle.</summary>
public enum BalanceCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A balance checkpoint as the balance team keeps it.</summary>
public sealed class BalanceCheckpoint(string code, string name, int leaveDays, string ownerId, int createdAt, double score, int expiresAt, string label, int version, string jobCode, double margin, BalanceCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public int CreatedAt { get; set; } = createdAt;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public double Margin { get; set; } = margin;
    public BalanceCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a balance checkpoint in status planned.</summary>
    public static BalanceCheckpoint NewBalanceCheckpoint(string code, string name)
    {
        return new BalanceCheckpoint(code, name, 0, "", 0, 0.0, 0, "", 0, "", 0.0, BalanceCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceCheckpointStatus next)
    {
        switch (Status)
        {
            case BalanceCheckpointStatus.Planned:
                return next == BalanceCheckpointStatus.Scheduled || next == BalanceCheckpointStatus.InProgress;
            case BalanceCheckpointStatus.Scheduled:
                return next == BalanceCheckpointStatus.Completed || next == BalanceCheckpointStatus.Cancelled;
            case BalanceCheckpointStatus.InProgress:
                switch (next)
                {
                    case BalanceCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceCheckpointStatus.Completed:
                return next == BalanceCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BalanceCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)LeaveDays * (double)ExpiresAt, 2);
    }

    public BalanceCheckpoint Copy()
    {
        return new BalanceCheckpoint(
            Code,
            Name,
            LeaveDays,
            OwnerId,
            CreatedAt,
            Score,
            ExpiresAt,
            Label,
            Version,
            JobCode,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
