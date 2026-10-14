namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work window is in its life cycle.</summary>
public enum StatementOfWorkWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a statement of work window and its current status.</summary>
public sealed class StatementOfWorkWindow(string code, string name, int batchSize, int expiresAt, string channel, string managerId, StatementOfWorkWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public string ManagerId { get; set; } = managerId;
    public StatementOfWorkWindowStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work window in status idle.</summary>
    public static StatementOfWorkWindow NewStatementOfWorkWindow(string code, string name)
    {
        return new StatementOfWorkWindow(code, name, 0, 0, "", "", StatementOfWorkWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkWindowStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkWindowStatus.Idle:
                switch (next)
                {
                    case StatementOfWorkWindowStatus.Ready:
                    case StatementOfWorkWindowStatus.Draining:
                        return true;
                }
                return false;
            case StatementOfWorkWindowStatus.Warming:
                switch (next)
                {
                    case StatementOfWorkWindowStatus.Stopped:
                        return true;
                }
                return false;
            case StatementOfWorkWindowStatus.Ready:
                return next == StatementOfWorkWindowStatus.Draining;
            case StatementOfWorkWindowStatus.Draining:
                return next == StatementOfWorkWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)BatchSize - (double)ExpiresAt, 2);
    }

    public StatementOfWorkWindow Copy()
    {
        return new StatementOfWorkWindow(Code, Name, BatchSize, ExpiresAt, Channel, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
