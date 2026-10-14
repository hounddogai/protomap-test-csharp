namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment variance calculation.</summary>
public interface IGarnishmentVarianceStep
{
    string Describe();
    double Adjust(GarnishmentVariance item, double valueValue);
}

public sealed record MinimumGarnishmentVarianceAdjuster(double Minimum) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGarnishmentVarianceAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentVarianceAdjuster(double Cap) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelGarnishmentVarianceAdjuster(Dictionary<string, double> Factors) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGarnishmentVarianceAdjuster(double Percent) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGarnishmentVarianceAdjuster(double Step) : IGarnishmentVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment variance steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentVarianceEngine(List<IGarnishmentVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentVarianceEngine DefaultGarnishmentVarianceEngine(Registry flags)
    {
        return new GarnishmentVarianceEngine(
            new List<IGarnishmentVarianceStep>
            {
                new MinimumGarnishmentVarianceAdjuster(1.2),
                new FlaggedGarnishmentVarianceAdjuster(flags, "garnishment-variance-manual", 0.7),
                new CapGarnishmentVarianceAdjuster(1207.0),
                new LabelGarnishmentVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.86, ["online"] = 1.2, ["north"] = 1.29 }),
                new PercentGarnishmentVarianceAdjuster(12.7),
                new RoundingGarnishmentVarianceAdjuster(0.25),
            },
            new List<string>(),
            2.2,
            61875.4);
    }

    public double Evaluate(GarnishmentVariance item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
