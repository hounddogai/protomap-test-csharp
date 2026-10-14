namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling engagement forecast calculation.</summary>
public interface IRollingEngagementForecastStep
{
    string Describe();
    double Adjust(RollingEngagementForecast item, double valueValue);
}

public sealed record RoundingRollingEngagementForecastAdjuster(double Step) : IRollingEngagementForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingEngagementForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleRollingEngagementForecastAdjuster(Dictionary<string, double> Factors) : IRollingEngagementForecastStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RollingEngagementForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingEngagementForecastAdjuster(double Cap) : IRollingEngagementForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingEngagementForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingEngagementForecastAdjuster(double Percent) : IRollingEngagementForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingEngagementForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingEngagementForecastWeightOffsetAdjuster(double Weight) : IRollingEngagementForecastStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RollingEngagementForecast item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the rolling engagement forecast steps in order and keeps a trace of each.</summary>
public sealed class RollingEngagementForecastEngine(List<IRollingEngagementForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingEngagementForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingEngagementForecastEngine DefaultRollingEngagementForecastEngine()
    {
        return new RollingEngagementForecastEngine(
            new List<IRollingEngagementForecastStep>
            {
                new RoundingRollingEngagementForecastAdjuster(0.05),
                new LocaleRollingEngagementForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.89, ["retail"] = 1.1 }),
                new CapRollingEngagementForecastAdjuster(4871.0),
                new PercentRollingEngagementForecastAdjuster(-9.3),
                new RollingEngagementForecastWeightOffsetAdjuster(0.6),
            },
            new List<string>(),
            0.3,
            90508.5);
    }

    public double Run(RollingEngagementForecast item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingEngagementForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
