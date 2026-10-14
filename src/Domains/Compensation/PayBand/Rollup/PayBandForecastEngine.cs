namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band forecast calculation.</summary>
public interface IPayBandForecastAdjuster
{
    string Describe();
    double Adjust(PayBandForecast item, double valueValue);
}

public sealed record RoundingPayBandForecastAdjuster(double Step) : IPayBandForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPayBandForecastAdjuster(Registry Flags, string Flag, double Percent) : IPayBandForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandForecastAdjuster(List<double> Bounds, List<double> Rates) : IPayBandForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandForecast item, double valueValue)
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

public sealed record PercentPayBandForecastAdjuster(double Percent) : IPayBandForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentPayBandForecastAdjuster(Dictionary<string, double> Factors) : IPayBandForecastAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayBandForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band forecast steps in order and keeps a trace of each.</summary>
public sealed class PayBandForecastEngine(List<IPayBandForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandForecastEngine DefaultPayBandForecastEngine(Registry flags)
    {
        return new PayBandForecastEngine(
            new List<IPayBandForecastAdjuster>
            {
                new RoundingPayBandForecastAdjuster(0.5),
                new FlaggedPayBandForecastAdjuster(flags, "pay-band-forecast-tiered", 5.5),
                new TieredPayBandForecastAdjuster(
                    new List<double> { 954.0, 2637.0 },
                    new List<double> { 0.95, 0.96 }),
                new PercentPayBandForecastAdjuster(15.3),
                new DepartmentPayBandForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.18, ["central"] = 1.24 }),
            },
            new List<string>(),
            0.2,
            7814.9);
    }

    public double Price(PayBandForecast item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
