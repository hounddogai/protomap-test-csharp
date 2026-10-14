namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool forecast calculation.</summary>
public interface IBonusPoolForecastAdjuster
{
    string Describe();
    double Adjust(BonusPoolForecast item, double valueValue);
}

public sealed record TieredBonusPoolForecastAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolForecast item, double valueValue)
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

public sealed record BonusPoolForecastRevisionOffsetAdjuster(double Weight) : IBonusPoolForecastAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(BonusPoolForecast item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record RoundingBonusPoolForecastAdjuster(double Step) : IBonusPoolForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBonusPoolForecastAdjuster(double Percent) : IBonusPoolForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleBonusPoolForecastAdjuster(Dictionary<string, double> Factors) : IBonusPoolForecastAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BonusPoolForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool forecast steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolForecastEngine(List<IBonusPoolForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolForecastEngine DefaultBonusPoolForecastEngine()
    {
        return new BonusPoolForecastEngine(
            new List<IBonusPoolForecastAdjuster>
            {
                new TieredBonusPoolForecastAdjuster(
                    new List<double> { 4024.0, 6066.0, 6326.0, 9602.0 },
                    new List<double> { 1.1, 0.93, 1.2, 1.13 }),
                new BonusPoolForecastRevisionOffsetAdjuster(1.14),
                new RoundingBonusPoolForecastAdjuster(0.1),
                new PercentBonusPoolForecastAdjuster(2.5),
                new TitleBonusPoolForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.25, ["west"] = 1.08, ["north"] = 1.21 }),
            },
            new List<string>(),
            3.9,
            10968.4);
    }

    public double Compute(BonusPoolForecast item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
