namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance rule is in its life cycle.</summary>
public enum BalanceRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A balance rule as the balance team keeps it.</summary>
public sealed class BalanceRule(string code, string title, double cost, string region, string label, string currency, string badge, double amount, double rate, int durationMinutes, BalanceRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Region { get; set; } = region;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BalanceRuleStatus Status { get; set; } = status;

    /// <summary>Starts a balance rule in status created.</summary>
    public static BalanceRule NewBalanceRule(string code, string title)
    {
        return new BalanceRule(code, title, 0.0, "", "", "", "", 0.0, 0.0, 0, BalanceRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceRuleStatus next)
    {
        switch (Status)
        {
            case BalanceRuleStatus.Created:
                switch (next)
                {
                    case BalanceRuleStatus.Retired:
                        return true;
                }
                return false;
            case BalanceRuleStatus.Validated:
                switch (next)
                {
                    case BalanceRuleStatus.Retired:
                        return true;
                }
                return false;
            case BalanceRuleStatus.Published:
                return next == BalanceRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BalanceRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)DurationMinutes - Cost, 2);
    }

    public BalanceRule Copy()
    {
        return new BalanceRule(
            Code,
            Title,
            Cost,
            Region,
            Label,
            Currency,
            Badge,
            Amount,
            Rate,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
