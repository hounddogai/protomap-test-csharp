namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool budget is in its life cycle.</summary>
public enum PoolBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pool budget as the pool team keeps it.</summary>
public sealed class PoolBudget(string code, string name, int batchSize, Dictionary<string, string> attributes, double ratio, string costCenter, string ownerId, string locale, double discount, double amount, int attempts, PoolBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public PoolBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a pool budget in status requested.</summary>
    public static PoolBudget NewPoolBudget(string code, string name)
    {
        return new PoolBudget(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            PoolBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolBudgetStatus next)
    {
        switch (Status)
        {
            case PoolBudgetStatus.Requested:
                switch (next)
                {
                    case PoolBudgetStatus.Granted:
                        return true;
                }
                return false;
            case PoolBudgetStatus.Granted:
                switch (next)
                {
                    case PoolBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case PoolBudgetStatus.Revoked:
                return next == PoolBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PoolBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double WeightedValue()
    {
        return Math.Round(Ratio - (double)Attempts, 2);
    }

    public PoolBudget Copy()
    {
        return new PoolBudget(
            Code,
            Name,
            BatchSize,
            Attributes,
            Ratio,
            CostCenter,
            OwnerId,
            Locale,
            Discount,
            Amount,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
