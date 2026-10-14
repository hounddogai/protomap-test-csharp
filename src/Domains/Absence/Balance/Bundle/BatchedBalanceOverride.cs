namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched balance override is in its life cycle.</summary>
public enum BatchedBalanceOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a batched balance override and its current status.</summary>
public sealed class BatchedBalanceOverride(string code, string name, double rate, double floorValue, string badge, BatchedBalanceOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double FloorValue { get; set; } = floorValue;
    public string Badge { get; set; } = badge;
    public BatchedBalanceOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a batched balance override in status planned.</summary>
    public static BatchedBalanceOverride NewBatchedBalanceOverride(string code, string name)
    {
        return new BatchedBalanceOverride(code, name, 0.0, 0.0, "", BatchedBalanceOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBalanceOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBalanceOverrideStatus next)
    {
        switch (Status)
        {
            case BatchedBalanceOverrideStatus.Planned:
                return next == BatchedBalanceOverrideStatus.Scheduled || next == BatchedBalanceOverrideStatus.Completed;
            case BatchedBalanceOverrideStatus.Scheduled:
                return next == BatchedBalanceOverrideStatus.InProgress;
            case BatchedBalanceOverrideStatus.InProgress:
                switch (next)
                {
                    case BatchedBalanceOverrideStatus.Completed:
                    case BatchedBalanceOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedBalanceOverrideStatus.Completed:
                return next == BatchedBalanceOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedBalanceOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched balance override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched balance override"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Rate * FloorValue, 2);
    }

    public BatchedBalanceOverride Copy()
    {
        return new BatchedBalanceOverride(Code, Name, Rate, FloorValue, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
