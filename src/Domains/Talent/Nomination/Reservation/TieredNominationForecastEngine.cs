namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered nomination forecast calculation.</summary>
public interface ITieredNominationForecastStep
{
    string Describe();
    double Adjust(TieredNominationForecast item, double valueValue);
}

public sealed record FlaggedTieredNominationForecastAdjuster(Registry Flags, string Flag, double Percent) : ITieredNominationForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredNominationForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredNominationForecastAdjuster(double Cap) : ITieredNominationForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredNominationForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameTieredNominationForecastAdjuster(Dictionary<string, double> Factors) : ITieredNominationForecastStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredNominationForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered nomination forecast steps in order and keeps a trace of each.</summary>
public sealed class TieredNominationForecastEngine(List<ITieredNominationForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredNominationForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredNominationForecastEngine DefaultTieredNominationForecastEngine(Registry flags)
    {
        return new TieredNominationForecastEngine(
            new List<ITieredNominationForecastStep>
            {
                new FlaggedTieredNominationForecastAdjuster(flags, "tiered-nomination-forecast-tiered", 2.5),
                new CapTieredNominationForecastAdjuster(33056.0),
                new NameTieredNominationForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.19, ["central"] = 0.79, ["north"] = 0.72 }),
            },
            new List<string>(),
            4.5,
            91412.6);
    }

    public double Price(TieredNominationForecast item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredNominationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
