namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary desk forecast calculation.</summary>
public interface IPrimaryDeskForecastAdjuster
{
    string Describe();
    double Adjust(PrimaryDeskForecast item, double valueValue);
}

public sealed record MinimumPrimaryDeskForecastAdjuster(double Minimum) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPrimaryDeskForecastAdjuster(double Step) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPrimaryDeskForecastAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
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

public sealed record PercentPrimaryDeskForecastAdjuster(double Percent) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryDeskForecastAdjuster(double Cap) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryDeskForecastAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryDeskForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryDeskForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary desk forecast steps in order and keeps a trace of each.</summary>
public sealed class PrimaryDeskForecastEngine(List<IPrimaryDeskForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryDeskForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryDeskForecastEngine DefaultPrimaryDeskForecastEngine(Registry flags)
    {
        return new PrimaryDeskForecastEngine(
            new List<IPrimaryDeskForecastAdjuster>
            {
                new MinimumPrimaryDeskForecastAdjuster(7.2),
                new RoundingPrimaryDeskForecastAdjuster(1.0),
                new TieredPrimaryDeskForecastAdjuster(
                    new List<double> { 537.0, 5317.0 },
                    new List<double> { 1.1, 1.04 }),
                new PercentPrimaryDeskForecastAdjuster(-5.3),
                new CapPrimaryDeskForecastAdjuster(35676.0),
                new FlaggedPrimaryDeskForecastAdjuster(flags, "primary-desk-forecast-primary", 2.3),
            },
            new List<string>(),
            3.0,
            30025.0);
    }

    public double Run(PrimaryDeskForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryDeskForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
