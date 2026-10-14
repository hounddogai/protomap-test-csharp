namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient rate forecast calculation.</summary>
public interface ILenientRateForecastAdjuster
{
    string Describe();
    double Adjust(LenientRateForecast item, double valueValue);
}

public sealed record MinimumLenientRateForecastAdjuster(double Minimum) : ILenientRateForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRateForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientRateForecastAdjuster(Registry Flags, string Flag, double Percent) : ILenientRateForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRateForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientRateForecastBudgetOffsetAdjuster(double Weight) : ILenientRateForecastAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(LenientRateForecast item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the lenient rate forecast steps in order and keeps a trace of each.</summary>
public sealed class LenientRateForecastEngine(List<ILenientRateForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRateForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRateForecastEngine DefaultLenientRateForecastEngine(Registry flags)
    {
        return new LenientRateForecastEngine(
            new List<ILenientRateForecastAdjuster>
            {
                new MinimumLenientRateForecastAdjuster(30.7),
                new FlaggedLenientRateForecastAdjuster(flags, "lenient-rate-forecast-legacy", -1.5),
                new LenientRateForecastBudgetOffsetAdjuster(0.56),
            },
            new List<string>(),
            3.0,
            4517.8);
    }

    public double Run(LenientRateForecast item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRateForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
