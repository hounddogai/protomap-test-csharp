namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pool exception calculation.</summary>
public interface IGlobalPoolExceptionAdjuster
{
    string Describe();
    double Adjust(GlobalPoolException item, double valueValue);
}

public sealed record RoundingGlobalPoolExceptionAdjuster(double Step) : IGlobalPoolExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPoolException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalPoolExceptionAdjuster(double Percent) : IGlobalPoolExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPoolException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalPoolExceptionAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPoolExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPoolException item, double valueValue)
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

public sealed record CountryGlobalPoolExceptionAdjuster(Dictionary<string, double> Factors) : IGlobalPoolExceptionAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GlobalPoolException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global pool exception steps in order and keeps a trace of each.</summary>
public sealed class GlobalPoolExceptionEngine(List<IGlobalPoolExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPoolExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPoolExceptionEngine DefaultGlobalPoolExceptionEngine()
    {
        return new GlobalPoolExceptionEngine(
            new List<IGlobalPoolExceptionAdjuster>
            {
                new RoundingGlobalPoolExceptionAdjuster(5.0),
                new PercentGlobalPoolExceptionAdjuster(-6.7),
                new TieredGlobalPoolExceptionAdjuster(
                    new List<double> { 1868.0, 7387.0 },
                    new List<double> { 1.15, 0.9 }),
                new CountryGlobalPoolExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.09, ["retail"] = 0.8, ["online"] = 0.86 }),
            },
            new List<string>(),
            3.9,
            95165.7);
    }

    public double Run(GlobalPoolException item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPoolException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
