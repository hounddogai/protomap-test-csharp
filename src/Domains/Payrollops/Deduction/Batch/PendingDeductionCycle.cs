namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending deduction cycle is in its life cycle.</summary>
public enum PendingDeductionCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pending deduction cycle as the deduction team keeps it.</summary>
public sealed class PendingDeductionCycle(string code, string label, int createdAt, double discount, Dictionary<string, string> attributes, int limit, PendingDeductionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Limit { get; set; } = limit;
    public PendingDeductionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pending deduction cycle in status open.</summary>
    public static PendingDeductionCycle NewPendingDeductionCycle(string code, string label)
    {
        return new PendingDeductionCycle(
            code,
            label,
            0,
            0.0,
            new Dictionary<string, string>(),
            0,
            PendingDeductionCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeductionCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeductionCycleStatus next)
    {
        switch (Status)
        {
            case PendingDeductionCycleStatus.Open:
                switch (next)
                {
                    case PendingDeductionCycleStatus.Reopened:
                        return true;
                }
                return false;
            case PendingDeductionCycleStatus.InReview:
                switch (next)
                {
                    case PendingDeductionCycleStatus.Archived:
                        return true;
                }
                return false;
            case PendingDeductionCycleStatus.Resolved:
                return next == PendingDeductionCycleStatus.Reopened;
            case PendingDeductionCycleStatus.Reopened:
                switch (next)
                {
                    case PendingDeductionCycleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingDeductionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending deduction cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending deduction cycle"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt + Discount, 2);
    }

    public PendingDeductionCycle Copy()
    {
        return new PendingDeductionCycle(Code, Label, CreatedAt, Discount, Attributes, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
