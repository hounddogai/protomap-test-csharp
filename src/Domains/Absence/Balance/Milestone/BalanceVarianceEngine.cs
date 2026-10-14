namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance variance calculation.</summary>
public interface IBalanceVarianceModifier
{
    string Describe();
    double Adjust(BalanceVariance item, double valueValue);
}

public sealed record TieredBalanceVarianceAdjuster(List<double> Bounds, List<double> Rates) : IBalanceVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceVariance item, double valueValue)
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

public sealed record RoundingBalanceVarianceAdjuster(double Step) : IBalanceVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BalanceVarianceAmountOffsetAdjuster(double Weight) : IBalanceVarianceModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BalanceVariance item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the balance variance steps in order and keeps a trace of each.</summary>
public sealed class BalanceVarianceEngine(List<IBalanceVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceVarianceEngine DefaultBalanceVarianceEngine()
    {
        return new BalanceVarianceEngine(
            new List<IBalanceVarianceModifier>
            {
                new TieredBalanceVarianceAdjuster(
                    new List<double> { 878.0, 5926.0, 6737.0, 7856.0 },
                    new List<double> { 1.12, 1.02, 1.11, 0.96 }),
                new RoundingBalanceVarianceAdjuster(1.0),
                new BalanceVarianceAmountOffsetAdjuster(0.66),
            },
            new List<string>(),
            3.0,
            5016.5);
    }

    public double Run(BalanceVariance item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
