namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan forecast calculation.</summary>
public interface IActionPlanForecastAdjuster
{
    string Describe();
    double Adjust(ActionPlanForecast item, double valueValue);
}

public sealed record TieredActionPlanForecastAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
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

public sealed record ActionPlanForecastRateOffsetAdjuster(double Weight) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingActionPlanForecastAdjuster(double Step) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentActionPlanForecastAdjuster(double Percent) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedActionPlanForecastAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumActionPlanForecastAdjuster(double Minimum) : IActionPlanForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the action plan forecast steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanForecastEngine(List<IActionPlanForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanForecastEngine DefaultActionPlanForecastEngine(Registry flags)
    {
        return new ActionPlanForecastEngine(
            new List<IActionPlanForecastAdjuster>
            {
                new TieredActionPlanForecastAdjuster(
                    new List<double> { 590.0, 2353.0 },
                    new List<double> { 0.81, 0.94 }),
                new ActionPlanForecastRateOffsetAdjuster(0.88),
                new RoundingActionPlanForecastAdjuster(1.0),
                new PercentActionPlanForecastAdjuster(17.6),
                new FlaggedActionPlanForecastAdjuster(flags, "action-plan-forecast-scheduled", -3.3),
                new MinimumActionPlanForecastAdjuster(26.8),
            },
            new List<string>(),
            0.6,
            94560.9);
    }

    public double Run(ActionPlanForecast item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
