namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary statement of work allocation is in its life cycle.</summary>
public enum PrimaryStatementOfWorkAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a primary statement of work allocation and its current status.</summary>
public sealed class PrimaryStatementOfWorkAllocation(string code, string label, string badge, int startsAt, string costCenter, string channel, bool approved, PrimaryStatementOfWorkAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public string CostCenter { get; set; } = costCenter;
    public string Channel { get; set; } = channel;
    public bool Approved { get; set; } = approved;
    public PrimaryStatementOfWorkAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a primary statement of work allocation in status idle.</summary>
    public static PrimaryStatementOfWorkAllocation NewPrimaryStatementOfWorkAllocation(string code, string label)
    {
        return new PrimaryStatementOfWorkAllocation(
            code,
            label,
            "",
            0,
            "",
            "",
            false,
            PrimaryStatementOfWorkAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryStatementOfWorkAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryStatementOfWorkAllocationStatus next)
    {
        switch (Status)
        {
            case PrimaryStatementOfWorkAllocationStatus.Idle:
                return next == PrimaryStatementOfWorkAllocationStatus.Ready;
            case PrimaryStatementOfWorkAllocationStatus.Warming:
                return next == PrimaryStatementOfWorkAllocationStatus.Ready || next == PrimaryStatementOfWorkAllocationStatus.Draining;
            case PrimaryStatementOfWorkAllocationStatus.Ready:
                switch (next)
                {
                    case PrimaryStatementOfWorkAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryStatementOfWorkAllocationStatus.Draining:
                return next == PrimaryStatementOfWorkAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryStatementOfWorkAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary statement of work allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary statement of work allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public PrimaryStatementOfWorkAllocation Copy()
    {
        return new PrimaryStatementOfWorkAllocation(
            Code,
            Label,
            Badge,
            StartsAt,
            CostCenter,
            Channel,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
