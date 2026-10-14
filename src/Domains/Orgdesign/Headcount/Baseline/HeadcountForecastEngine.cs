namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount forecast calculation.</summary>
public interface IHeadcountForecastStep
{
    string Describe();
    double Adjust(HeadcountForecast item, double valueValue);
}

public sealed record FlaggedHeadcountForecastAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameHeadcountForecastAdjuster(Dictionary<string, double> Factors) : IHeadcountForecastStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(HeadcountForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapHeadcountForecastAdjuster(double Cap) : IHeadcountForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingHeadcountForecastAdjuster(double Step) : IHeadcountForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHeadcountForecastAdjuster(double Minimum) : IHeadcountForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the headcount forecast steps in order and keeps a trace of each.</summary>
public sealed class HeadcountForecastEngine(List<IHeadcountForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountForecastEngine DefaultHeadcountForecastEngine(Registry flags)
    {
        return new HeadcountForecastEngine(
            new List<IHeadcountForecastStep>
            {
                new FlaggedHeadcountForecastAdjuster(flags, "headcount-forecast-seasonal", -2.3),
                new NameHeadcountForecastAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.25, ["west"] = 0.84, ["online"] = 0.97 }),
                new CapHeadcountForecastAdjuster(12662.0),
                new RoundingHeadcountForecastAdjuster(0.05),
                new MinimumHeadcountForecastAdjuster(28.7),
            },
            new List<string>(),
            1.0,
            62909.4);
    }

    public double Evaluate(HeadcountForecast item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
