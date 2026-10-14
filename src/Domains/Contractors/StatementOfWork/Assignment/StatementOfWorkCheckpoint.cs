namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work checkpoint is in its life cycle.</summary>
public enum StatementOfWorkCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A statement of work checkpoint as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkCheckpoint(string code, string title, double maximum, string badge, int startsAt, StatementOfWorkCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public StatementOfWorkCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work checkpoint in status idle.</summary>
    public static StatementOfWorkCheckpoint NewStatementOfWorkCheckpoint(string code, string title)
    {
        return new StatementOfWorkCheckpoint(code, title, 0.0, "", 0, StatementOfWorkCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkCheckpointStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkCheckpointStatus.Idle:
                return next == StatementOfWorkCheckpointStatus.Warming || next == StatementOfWorkCheckpointStatus.Ready;
            case StatementOfWorkCheckpointStatus.Warming:
                switch (next)
                {
                    case StatementOfWorkCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case StatementOfWorkCheckpointStatus.Ready:
                switch (next)
                {
                    case StatementOfWorkCheckpointStatus.Draining:
                    case StatementOfWorkCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case StatementOfWorkCheckpointStatus.Draining:
                return next == StatementOfWorkCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"statement of work checkpoint"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Maximum + (double)StartsAt, 2);
    }

    public StatementOfWorkCheckpoint Copy()
    {
        return new StatementOfWorkCheckpoint(Code, Title, Maximum, Badge, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
