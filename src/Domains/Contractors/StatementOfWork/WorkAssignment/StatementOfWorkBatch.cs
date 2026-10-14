namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work batch is in its life cycle.</summary>
public enum StatementOfWorkBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a statement of work batch and its current status.</summary>
public sealed class StatementOfWorkBatch(string code, string label, int graceDays, int priority, bool approved, double ceiling, string externalRef, StatementOfWorkBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public StatementOfWorkBatchStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work batch in status idle.</summary>
    public static StatementOfWorkBatch NewStatementOfWorkBatch(string code, string label)
    {
        return new StatementOfWorkBatch(code, label, 0, 0, false, 0.0, "", StatementOfWorkBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkBatchStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkBatchStatus.Idle:
                return next == StatementOfWorkBatchStatus.Draining;
            case StatementOfWorkBatchStatus.Warming:
                switch (next)
                {
                    case StatementOfWorkBatchStatus.Draining:
                    case StatementOfWorkBatchStatus.Stopped:
                        return true;
                }
                return false;
            case StatementOfWorkBatchStatus.Ready:
                switch (next)
                {
                    case StatementOfWorkBatchStatus.Draining:
                        return true;
                }
                return false;
            case StatementOfWorkBatchStatus.Draining:
                switch (next)
                {
                    case StatementOfWorkBatchStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority * Ceiling, 2);
    }

    public StatementOfWorkBatch Copy()
    {
        return new StatementOfWorkBatch(Code, Label, GraceDays, Priority, Approved, Ceiling, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
