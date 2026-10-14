namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance forecast calculation.</summary>
public interface IBalanceForecastStep
{
    string Describe();
    double Adjust(BalanceForecast item, double valueValue);
}

public sealed record BalanceForecastMarginOffsetAdjuster(double Weight) : IBalanceForecastStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(BalanceForecast item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredBalanceForecastAdjuster(List<double> Bounds, List<double> Rates) : IBalanceForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceForecast item, double valueValue)
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

public sealed record RoundingBalanceForecastAdjuster(double Step) : IBalanceForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBalanceForecastAdjuster(double Percent) : IBalanceForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the balance forecast steps in order and keeps a trace of each.</summary>
public sealed class BalanceForecastEngine(List<IBalanceForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceForecastEngine DefaultBalanceForecastEngine()
    {
        return new BalanceForecastEngine(
            new List<IBalanceForecastStep>
            {
                new BalanceForecastMarginOffsetAdjuster(1.25),
                new TieredBalanceForecastAdjuster(
                    new List<double> { 2542.0, 6144.0 },
                    new List<double> { 1.15, 0.88 }),
                new RoundingBalanceForecastAdjuster(0.25),
                new PercentBalanceForecastAdjuster(-5.8),
            },
            new List<string>(),
            2.8,
            41229.1);
    }

    public double Compute(BalanceForecast item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
