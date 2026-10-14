namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback balance claim calculation.</summary>
public interface IFallbackBalanceClaimFactor
{
    string Describe();
    double Adjust(FallbackBalanceClaim item, double valueValue);
}

public sealed record FallbackBalanceClaimBudgetOffsetAdjuster(double Weight) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredFallbackBalanceClaimAdjuster(List<double> Bounds, List<double> Rates) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
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

public sealed record RoundingFallbackBalanceClaimAdjuster(double Step) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitFallbackBalanceClaimAdjuster(Dictionary<string, double> Factors) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackBalanceClaimAdjuster(double Percent) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackBalanceClaimAdjuster(Registry Flags, string Flag, double Percent) : IFallbackBalanceClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackBalanceClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback balance claim steps in order and keeps a trace of each.</summary>
public sealed class FallbackBalanceClaimEngine(List<IFallbackBalanceClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackBalanceClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackBalanceClaimEngine DefaultFallbackBalanceClaimEngine(Registry flags)
    {
        return new FallbackBalanceClaimEngine(
            new List<IFallbackBalanceClaimFactor>
            {
                new FallbackBalanceClaimBudgetOffsetAdjuster(0.99),
                new TieredFallbackBalanceClaimAdjuster(
                    new List<double> { 2395.0, 4763.0 },
                    new List<double> { 1.05, 0.98 }),
                new RoundingFallbackBalanceClaimAdjuster(0.25),
                new UnitFallbackBalanceClaimAdjuster(new Dictionary<string, double> { ["north"] = 1.39 }),
                new PercentFallbackBalanceClaimAdjuster(-5.1),
                new FlaggedFallbackBalanceClaimAdjuster(flags, "fallback-balance-claim-manual", -5.5),
            },
            new List<string>(),
            0.4,
            62396.1);
    }

    public double Evaluate(FallbackBalanceClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackBalanceClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
