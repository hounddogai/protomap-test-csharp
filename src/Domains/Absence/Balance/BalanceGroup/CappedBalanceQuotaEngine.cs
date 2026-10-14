namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped balance quota calculation.</summary>
public interface ICappedBalanceQuotaFactor
{
    string Describe();
    double Adjust(CappedBalanceQuota item, double valueValue);
}

public sealed record TieredCappedBalanceQuotaAdjuster(List<double> Bounds, List<double> Rates) : ICappedBalanceQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedBalanceQuota item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumCappedBalanceQuotaAdjuster(double Minimum) : ICappedBalanceQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedBalanceQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedBalanceQuotaAdjuster(Registry Flags, string Flag, double Percent) : ICappedBalanceQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedBalanceQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedBalanceQuotaAdjuster(double Cap) : ICappedBalanceQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBalanceQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped balance quota steps in order and keeps a trace of each.</summary>
public sealed class CappedBalanceQuotaEngine(List<ICappedBalanceQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBalanceQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBalanceQuotaEngine DefaultCappedBalanceQuotaEngine(Registry flags)
    {
        return new CappedBalanceQuotaEngine(
            new List<ICappedBalanceQuotaFactor>
            {
                new TieredCappedBalanceQuotaAdjuster(
                    new List<double> { 1427.0, 6006.0 },
                    new List<double> { 1.03, 0.95 }),
                new MinimumCappedBalanceQuotaAdjuster(43.5),
                new FlaggedCappedBalanceQuotaAdjuster(flags, "capped-balance-quota-adaptive", 3.3),
                new CapCappedBalanceQuotaAdjuster(38837.0),
            },
            new List<string>(),
            3.7,
            74394.9);
    }

    public double Run(CappedBalanceQuota item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedBalanceQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
