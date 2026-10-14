namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy vendor budget is in its life cycle.</summary>
public enum LegacyVendorBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy vendor budget. Its code is unique within a tenant.</summary>
public sealed class LegacyVendorBudget(string code, string name, double amount, double weight, string region, string unit, LegacyVendorBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public string Unit { get; set; } = unit;
    public LegacyVendorBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy vendor budget in status idle.</summary>
    public static LegacyVendorBudget NewLegacyVendorBudget(string code, string name)
    {
        return new LegacyVendorBudget(code, name, 0.0, 0.0, "", "", LegacyVendorBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyVendorBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyVendorBudgetStatus next)
    {
        switch (Status)
        {
            case LegacyVendorBudgetStatus.Idle:
                switch (next)
                {
                    case LegacyVendorBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyVendorBudgetStatus.Warming:
                switch (next)
                {
                    case LegacyVendorBudgetStatus.Ready:
                    case LegacyVendorBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyVendorBudgetStatus.Ready:
                switch (next)
                {
                    case LegacyVendorBudgetStatus.Draining:
                    case LegacyVendorBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyVendorBudgetStatus.Draining:
                return next == LegacyVendorBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LegacyVendorBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy vendor budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy vendor budget"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Amount * Weight, 2);
    }

    public LegacyVendorBudget Copy()
    {
        return new LegacyVendorBudget(Code, Name, Amount, Weight, Region, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
