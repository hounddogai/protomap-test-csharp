namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient garnishment budget is in its life cycle.</summary>
public enum LenientGarnishmentBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient garnishment budget as the garnishment team keeps it.</summary>
public sealed class LenientGarnishmentBudget(string code, string label, string checksum, int priority, double amount, double maximum, List<string> tags, string badge, LenientGarnishmentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int Priority { get; set; } = priority;
    public double Amount { get; set; } = amount;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public string Badge { get; set; } = badge;
    public LenientGarnishmentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient garnishment budget in status idle.</summary>
    public static LenientGarnishmentBudget NewLenientGarnishmentBudget(string code, string label)
    {
        return new LenientGarnishmentBudget(
            code,
            label,
            "",
            0,
            0.0,
            0.0,
            new List<string>(),
            "",
            LenientGarnishmentBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientGarnishmentBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientGarnishmentBudgetStatus next)
    {
        switch (Status)
        {
            case LenientGarnishmentBudgetStatus.Idle:
                return next == LenientGarnishmentBudgetStatus.Ready || next == LenientGarnishmentBudgetStatus.Draining;
            case LenientGarnishmentBudgetStatus.Warming:
                return next == LenientGarnishmentBudgetStatus.Ready || next == LenientGarnishmentBudgetStatus.Stopped;
            case LenientGarnishmentBudgetStatus.Ready:
                switch (next)
                {
                    case LenientGarnishmentBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case LenientGarnishmentBudgetStatus.Draining:
                return next == LenientGarnishmentBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientGarnishmentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient garnishment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedExposure()
    {
        return Math.Round(Amount + Maximum, 2);
    }

    public LenientGarnishmentBudget Copy()
    {
        return new LenientGarnishmentBudget(Code, Label, Checksum, Priority, Amount, Maximum, Tags, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
