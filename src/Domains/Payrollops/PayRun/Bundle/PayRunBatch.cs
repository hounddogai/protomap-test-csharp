namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run batch is in its life cycle.</summary>
public enum PayRunBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pay run batch and its current status.</summary>
public sealed class PayRunBatch(string code, string label, double ratio, string badge, string description, int expiresAt, string reviewer, PayRunBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public PayRunBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pay run batch in status planned.</summary>
    public static PayRunBatch NewPayRunBatch(string code, string label)
    {
        return new PayRunBatch(code, label, 0.0, "", "", 0, "", PayRunBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunBatchStatus next)
    {
        switch (Status)
        {
            case PayRunBatchStatus.Planned:
                return next == PayRunBatchStatus.InProgress;
            case PayRunBatchStatus.Scheduled:
                switch (next)
                {
                    case PayRunBatchStatus.InProgress:
                    case PayRunBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case PayRunBatchStatus.InProgress:
                return next == PayRunBatchStatus.Completed || next == PayRunBatchStatus.Cancelled;
            case PayRunBatchStatus.Completed:
                switch (next)
                {
                    case PayRunBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pay run batch"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt + Ratio, 2);
    }

    public PayRunBatch Copy()
    {
        return new PayRunBatch(Code, Label, Ratio, Badge, Description, ExpiresAt, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
