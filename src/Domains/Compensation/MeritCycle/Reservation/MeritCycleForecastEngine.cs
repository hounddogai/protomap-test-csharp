namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle forecast calculation.</summary>
public interface IMeritCycleForecastStep
{
    string Describe();
    double Adjust(MeritCycleForecast item, double valueValue);
}

public sealed record CapMeritCycleForecastAdjuster(double Cap) : IMeritCycleForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedMeritCycleForecastAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleMeritCycleForecastAdjuster(Dictionary<string, double> Factors) : IMeritCycleForecastStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(MeritCycleForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle forecast steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleForecastEngine(List<IMeritCycleForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleForecastEngine DefaultMeritCycleForecastEngine(Registry flags)
    {
        return new MeritCycleForecastEngine(
            new List<IMeritCycleForecastStep>
            {
                new CapMeritCycleForecastAdjuster(33485.0),
                new FlaggedMeritCycleForecastAdjuster(flags, "merit-cycle-forecast-rolling", -3.2),
                new TitleMeritCycleForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.4, ["north"] = 1.31, ["central"] = 1.17 }),
            },
            new List<string>(),
            4.9,
            86321.7);
    }

    public double Evaluate(MeritCycleForecast item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
