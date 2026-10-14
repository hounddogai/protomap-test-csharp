namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run allocation is in its life cycle.</summary>
public enum PayRunAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pay run allocation and its current status.</summary>
public sealed class PayRunAllocation(string code, string label, string managerId, string unit, double score, PayRunAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public PayRunAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pay run allocation in status created.</summary>
    public static PayRunAllocation NewPayRunAllocation(string code, string label)
    {
        return new PayRunAllocation(code, label, "", "", 0.0, PayRunAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunAllocationStatus next)
    {
        switch (Status)
        {
            case PayRunAllocationStatus.Created:
                switch (next)
                {
                    case PayRunAllocationStatus.Validated:
                    case PayRunAllocationStatus.Published:
                        return true;
                }
                return false;
            case PayRunAllocationStatus.Validated:
                return next == PayRunAllocationStatus.Published || next == PayRunAllocationStatus.Retired;
            case PayRunAllocationStatus.Published:
                return next == PayRunAllocationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PayRunAllocation Copy()
    {
        return new PayRunAllocation(Code, Label, ManagerId, Unit, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
