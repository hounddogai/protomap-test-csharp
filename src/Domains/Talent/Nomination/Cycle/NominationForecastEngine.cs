namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination forecast calculation.</summary>
public interface INominationForecastStep
{
    string Describe();
    double Adjust(NominationForecast item, double valueValue);
}

public sealed record PercentNominationForecastAdjuster(double Percent) : INominationForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationForecastAdjuster(List<double> Bounds, List<double> Rates) : INominationForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationForecast item, double valueValue)
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

public sealed record CategoryNominationForecastAdjuster(Dictionary<string, double> Factors) : INominationForecastStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(NominationForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumNominationForecastAdjuster(double Minimum) : INominationForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingNominationForecastAdjuster(double Step) : INominationForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapNominationForecastAdjuster(double Cap) : INominationForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the nomination forecast steps in order and keeps a trace of each.</summary>
public sealed class NominationForecastEngine(List<INominationForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationForecastEngine DefaultNominationForecastEngine()
    {
        return new NominationForecastEngine(
            new List<INominationForecastStep>
            {
                new PercentNominationForecastAdjuster(15.9),
                new TieredNominationForecastAdjuster(
                    new List<double> { 129.0, 165.0, 1700.0, 6048.0 },
                    new List<double> { 1.03, 1.0, 1.12, 0.94 }),
                new CategoryNominationForecastAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.05, ["retail"] = 0.94 }),
                new MinimumNominationForecastAdjuster(1.6),
                new RoundingNominationForecastAdjuster(5.0),
                new CapNominationForecastAdjuster(24140.0),
            },
            new List<string>(),
            2.2,
            80680.8);
    }

    public double Compute(NominationForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
