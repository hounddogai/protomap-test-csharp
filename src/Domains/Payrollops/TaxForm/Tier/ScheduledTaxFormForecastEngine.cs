namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled tax form forecast calculation.</summary>
public interface IScheduledTaxFormForecastStep
{
    string Describe();
    double Adjust(ScheduledTaxFormForecast item, double valueValue);
}

public sealed record FlaggedScheduledTaxFormForecastAdjuster(Registry Flags, string Flag, double Percent) : IScheduledTaxFormForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledTaxFormForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledTaxFormForecastAdjuster(double Minimum) : IScheduledTaxFormForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledTaxFormForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleScheduledTaxFormForecastAdjuster(Dictionary<string, double> Factors) : IScheduledTaxFormForecastStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledTaxFormForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingScheduledTaxFormForecastAdjuster(double Step) : IScheduledTaxFormForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTaxFormForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled tax form forecast steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTaxFormForecastEngine(List<IScheduledTaxFormForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTaxFormForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTaxFormForecastEngine DefaultScheduledTaxFormForecastEngine(Registry flags)
    {
        return new ScheduledTaxFormForecastEngine(
            new List<IScheduledTaxFormForecastStep>
            {
                new FlaggedScheduledTaxFormForecastAdjuster(flags, "scheduled-tax-form-forecast-adaptive", -9.9),
                new MinimumScheduledTaxFormForecastAdjuster(13.4),
                new TitleScheduledTaxFormForecastAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.1, ["west"] = 1.15, ["online"] = 1.21 }),
                new RoundingScheduledTaxFormForecastAdjuster(0.05),
            },
            new List<string>(),
            2.2,
            76337.5);
    }

    public double Run(ScheduledTaxFormForecast item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTaxFormForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
