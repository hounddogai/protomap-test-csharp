namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop rule is in its life cycle.</summary>
public enum LaptopRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One laptop rule, with what the planners need to act on it.</summary>
public sealed class LaptopRule(string code, string title, string timezone, string unit, string costCenter, double amount, bool approved, string location, LaptopRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public double Amount { get; set; } = amount;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public LaptopRuleStatus Status { get; set; } = status;

    /// <summary>Starts a laptop rule in status idle.</summary>
    public static LaptopRule NewLaptopRule(string code, string title)
    {
        return new LaptopRule(code, title, "", "", "", 0.0, false, "", LaptopRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopRuleStatus next)
    {
        switch (Status)
        {
            case LaptopRuleStatus.Idle:
                switch (next)
                {
                    case LaptopRuleStatus.Draining:
                        return true;
                }
                return false;
            case LaptopRuleStatus.Warming:
                switch (next)
                {
                    case LaptopRuleStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopRuleStatus.Ready:
                return next == LaptopRuleStatus.Draining || next == LaptopRuleStatus.Stopped;
            case LaptopRuleStatus.Draining:
                switch (next)
                {
                    case LaptopRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LaptopRule Copy()
    {
        return new LaptopRule(Code, Title, Timezone, Unit, CostCenter, Amount, Approved, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
