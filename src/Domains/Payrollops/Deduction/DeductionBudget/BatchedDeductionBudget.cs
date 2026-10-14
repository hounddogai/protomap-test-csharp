namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched deduction budget is in its life cycle.</summary>
public enum BatchedDeductionBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched deduction budget. Its code is unique within a tenant.</summary>
public sealed class BatchedDeductionBudget(string code, string label, int expiresAt, Dictionary<string, string> attributes, string employeeId, double budget, BatchedDeductionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public double Budget { get; set; } = budget;
    public BatchedDeductionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a batched deduction budget in status requested.</summary>
    public static BatchedDeductionBudget NewBatchedDeductionBudget(string code, string label)
    {
        return new BatchedDeductionBudget(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            BatchedDeductionBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedDeductionBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedDeductionBudgetStatus next)
    {
        switch (Status)
        {
            case BatchedDeductionBudgetStatus.Requested:
                switch (next)
                {
                    case BatchedDeductionBudgetStatus.Granted:
                        return true;
                }
                return false;
            case BatchedDeductionBudgetStatus.Granted:
                return next == BatchedDeductionBudgetStatus.Revoked;
            case BatchedDeductionBudgetStatus.Revoked:
                return next == BatchedDeductionBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedDeductionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched deduction budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"batched deduction budget"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt + Budget, 2);
    }

    public BatchedDeductionBudget Copy()
    {
        return new BatchedDeductionBudget(Code, Label, ExpiresAt, Attributes, EmployeeId, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
