namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback statement of work checkpoint is in its life cycle.</summary>
public enum FallbackStatementOfWorkCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One fallback statement of work checkpoint, with what the planners need to act on it.</summary>
public sealed class FallbackStatementOfWorkCheckpoint(string code, string title, int expiresAt, int graceDays, string unit, bool archived, double score, bool pinned, bool approved, FallbackStatementOfWorkCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public string Unit { get; set; } = unit;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public bool Approved { get; set; } = approved;
    public FallbackStatementOfWorkCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a fallback statement of work checkpoint in status idle.</summary>
    public static FallbackStatementOfWorkCheckpoint NewFallbackStatementOfWorkCheckpoint(string code, string title)
    {
        return new FallbackStatementOfWorkCheckpoint(
            code,
            title,
            0,
            0,
            "",
            false,
            0.0,
            false,
            false,
            FallbackStatementOfWorkCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackStatementOfWorkCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackStatementOfWorkCheckpointStatus next)
    {
        switch (Status)
        {
            case FallbackStatementOfWorkCheckpointStatus.Idle:
                return next == FallbackStatementOfWorkCheckpointStatus.Warming || next == FallbackStatementOfWorkCheckpointStatus.Ready;
            case FallbackStatementOfWorkCheckpointStatus.Warming:
                switch (next)
                {
                    case FallbackStatementOfWorkCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackStatementOfWorkCheckpointStatus.Ready:
                return next == FallbackStatementOfWorkCheckpointStatus.Draining;
            case FallbackStatementOfWorkCheckpointStatus.Draining:
                return next == FallbackStatementOfWorkCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackStatementOfWorkCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback statement of work checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback statement of work checkpoint"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt + Score, 2);
    }

    public FallbackStatementOfWorkCheckpoint Copy()
    {
        return new FallbackStatementOfWorkCheckpoint(
            Code,
            Title,
            ExpiresAt,
            GraceDays,
            Unit,
            Archived,
            Score,
            Pinned,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
