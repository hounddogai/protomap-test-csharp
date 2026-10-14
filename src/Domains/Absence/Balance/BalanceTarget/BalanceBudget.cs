namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance budget is in its life cycle.</summary>
public enum BalanceBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a balance budget and its current status.</summary>
public sealed class BalanceBudget(string code, string label, string name, string jobCode, Dictionary<string, string> attributes, double minimum, double fte, double budget, string location, int priority, BalanceBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public int Priority { get; set; } = priority;
    public BalanceBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a balance budget in status planned.</summary>
    public static BalanceBudget NewBalanceBudget(string code, string label)
    {
        return new BalanceBudget(
            code,
            label,
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0.0,
            "",
            0,
            BalanceBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceBudgetStatus next)
    {
        switch (Status)
        {
            case BalanceBudgetStatus.Planned:
                switch (next)
                {
                    case BalanceBudgetStatus.Completed:
                        return true;
                }
                return false;
            case BalanceBudgetStatus.Scheduled:
                return next == BalanceBudgetStatus.InProgress || next == BalanceBudgetStatus.Cancelled;
            case BalanceBudgetStatus.InProgress:
                return next == BalanceBudgetStatus.Completed || next == BalanceBudgetStatus.Cancelled;
            case BalanceBudgetStatus.Completed:
                switch (next)
                {
                    case BalanceBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetValue()
    {
        return Math.Round((double)Priority + Minimum, 2);
    }

    public BalanceBudget Copy()
    {
        return new BalanceBudget(
            Code,
            Label,
            Name,
            JobCode,
            Attributes,
            Minimum,
            Fte,
            Budget,
            Location,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
