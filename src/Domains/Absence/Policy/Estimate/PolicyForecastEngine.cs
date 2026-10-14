namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy forecast calculation.</summary>
public interface IPolicyForecastFactor
{
    string Describe();
    double Adjust(PolicyForecast item, double valueValue);
}

public sealed record PolicyForecastScoreOffsetAdjuster(double Weight) : IPolicyForecastFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PolicyForecast item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingPolicyForecastAdjuster(double Step) : IPolicyForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPolicyForecastAdjuster(List<double> Bounds, List<double> Rates) : IPolicyForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyForecast item, double valueValue)
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

public sealed record MinimumPolicyForecastAdjuster(double Minimum) : IPolicyForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPolicyForecastAdjuster(double Percent) : IPolicyForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy forecast steps in order and keeps a trace of each.</summary>
public sealed class PolicyForecastEngine(List<IPolicyForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyForecastEngine DefaultPolicyForecastEngine()
    {
        return new PolicyForecastEngine(
            new List<IPolicyForecastFactor>
            {
                new PolicyForecastScoreOffsetAdjuster(1.09),
                new RoundingPolicyForecastAdjuster(0.05),
                new TieredPolicyForecastAdjuster(
                    new List<double> { 758.0, 3995.0, 8458.0 },
                    new List<double> { 0.96, 0.96, 1.14 }),
                new MinimumPolicyForecastAdjuster(22.7),
                new PercentPolicyForecastAdjuster(14.8),
            },
            new List<string>(),
            2.1,
            42704.4);
    }

    public double Run(PolicyForecast item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PolicyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
