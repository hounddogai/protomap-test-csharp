namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model allocation is in its life cycle.</summary>
public enum CostModelAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A cost model allocation as the cost model team keeps it.</summary>
public sealed class CostModelAllocation(string code, string label, bool archived, string jobCode, string location, double score, CostModelAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public CostModelAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a cost model allocation in status idle.</summary>
    public static CostModelAllocation NewCostModelAllocation(string code, string label)
    {
        return new CostModelAllocation(code, label, false, "", "", 0.0, CostModelAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelAllocationStatus next)
    {
        switch (Status)
        {
            case CostModelAllocationStatus.Idle:
                return next == CostModelAllocationStatus.Draining || next == CostModelAllocationStatus.Stopped;
            case CostModelAllocationStatus.Warming:
                switch (next)
                {
                    case CostModelAllocationStatus.Draining:
                        return true;
                }
                return false;
            case CostModelAllocationStatus.Ready:
                return next == CostModelAllocationStatus.Stopped;
            case CostModelAllocationStatus.Draining:
                return next == CostModelAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CostModelAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model allocation {Code} cannot move from {Status} to {next}");
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

    public CostModelAllocation Copy()
    {
        return new CostModelAllocation(Code, Label, Archived, JobCode, Location, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
