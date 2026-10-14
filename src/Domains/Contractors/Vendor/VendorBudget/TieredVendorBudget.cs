namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered vendor budget is in its life cycle.</summary>
public enum TieredVendorBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered vendor budget. Its code is unique within a tenant.</summary>
public sealed class TieredVendorBudget(string code, string label, int expiresAt, int sequence, int priority, TieredVendorBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Sequence { get; set; } = sequence;
    public int Priority { get; set; } = priority;
    public TieredVendorBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered vendor budget in status draft.</summary>
    public static TieredVendorBudget NewTieredVendorBudget(string code, string label)
    {
        return new TieredVendorBudget(code, label, 0, 0, 0, TieredVendorBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredVendorBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredVendorBudgetStatus next)
    {
        switch (Status)
        {
            case TieredVendorBudgetStatus.Draft:
                return next == TieredVendorBudgetStatus.Pending || next == TieredVendorBudgetStatus.Active;
            case TieredVendorBudgetStatus.Pending:
                switch (next)
                {
                    case TieredVendorBudgetStatus.Active:
                    case TieredVendorBudgetStatus.Closed:
                        return true;
                }
                return false;
            case TieredVendorBudgetStatus.Active:
                return next == TieredVendorBudgetStatus.Suspended;
            case TieredVendorBudgetStatus.Suspended:
                return next == TieredVendorBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredVendorBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered vendor budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Sequence - (double)Priority, 2);
    }

    public TieredVendorBudget Copy()
    {
        return new TieredVendorBudget(Code, Label, ExpiresAt, Sequence, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
