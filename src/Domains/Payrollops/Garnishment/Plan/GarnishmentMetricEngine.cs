namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment metric calculation.</summary>
public interface IGarnishmentMetricFactor
{
    string Describe();
    double Adjust(GarnishmentMetric item, double valueValue);
}

public sealed record CountryGarnishmentMetricAdjuster(Dictionary<string, double> Factors) : IGarnishmentMetricFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GarnishmentMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGarnishmentMetricAdjuster(double Step) : IGarnishmentMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGarnishmentMetricAdjuster(double Cap) : IGarnishmentMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGarnishmentMetricAdjuster(List<double> Bounds, List<double> Rates) : IGarnishmentMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GarnishmentMetric item, double valueValue)
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

public sealed record MinimumGarnishmentMetricAdjuster(double Minimum) : IGarnishmentMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the garnishment metric steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentMetricEngine(List<IGarnishmentMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentMetricEngine DefaultGarnishmentMetricEngine()
    {
        return new GarnishmentMetricEngine(
            new List<IGarnishmentMetricFactor>
            {
                new CountryGarnishmentMetricAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.14, ["north"] = 1.38, ["central"] = 1.27 }),
                new RoundingGarnishmentMetricAdjuster(0.25),
                new CapGarnishmentMetricAdjuster(3674.0),
                new TieredGarnishmentMetricAdjuster(
                    new List<double> { 5095.0, 6626.0, 9643.0 },
                    new List<double> { 0.87, 0.95, 0.94 }),
                new MinimumGarnishmentMetricAdjuster(24.7),
            },
            new List<string>(),
            2.6,
            74957.0);
    }

    public double Run(GarnishmentMetric item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
