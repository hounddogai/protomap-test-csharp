namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient nomination forecast calculation.</summary>
public interface ILenientNominationForecastStep
{
    string Describe();
    double Adjust(LenientNominationForecast item, double valueValue);
}

public sealed record LenientNominationForecastMaximumOffsetAdjuster(double Weight) : ILenientNominationForecastStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LenientNominationForecast item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentLenientNominationForecastAdjuster(double Percent) : ILenientNominationForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientNominationForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientNominationForecastAdjuster(double Step) : ILenientNominationForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientNominationForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitLenientNominationForecastAdjuster(Dictionary<string, double> Factors) : ILenientNominationForecastStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LenientNominationForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient nomination forecast steps in order and keeps a trace of each.</summary>
public sealed class LenientNominationForecastEngine(List<ILenientNominationForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientNominationForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientNominationForecastEngine DefaultLenientNominationForecastEngine()
    {
        return new LenientNominationForecastEngine(
            new List<ILenientNominationForecastStep>
            {
                new LenientNominationForecastMaximumOffsetAdjuster(1.97),
                new PercentLenientNominationForecastAdjuster(-0.5),
                new RoundingLenientNominationForecastAdjuster(0.1),
                new UnitLenientNominationForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.12, ["west"] = 0.79, ["south"] = 1.07 }),
            },
            new List<string>(),
            1.8,
            41979.2);
    }

    public double Run(LenientNominationForecast item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientNominationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
