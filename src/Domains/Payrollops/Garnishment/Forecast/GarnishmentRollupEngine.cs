namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment rollup calculation.</summary>
public interface IGarnishmentRollupStep
{
    string Describe();
    double Adjust(GarnishmentRollup item, double valueValue);
}

public sealed record RoundingGarnishmentRollupAdjuster(double Step) : IGarnishmentRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGarnishmentRollupAdjuster(double Cap) : IGarnishmentRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelGarnishmentRollupAdjuster(Dictionary<string, double> Factors) : IGarnishmentRollupStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GarnishmentRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGarnishmentRollupAdjuster(double Minimum) : IGarnishmentRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentGarnishmentRollupAdjuster(double Percent) : IGarnishmentRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment rollup steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentRollupEngine(List<IGarnishmentRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentRollupEngine DefaultGarnishmentRollupEngine()
    {
        return new GarnishmentRollupEngine(
            new List<IGarnishmentRollupStep>
            {
                new RoundingGarnishmentRollupAdjuster(0.5),
                new CapGarnishmentRollupAdjuster(39888.0),
                new LabelGarnishmentRollupAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.34, ["east"] = 1.33 }),
                new MinimumGarnishmentRollupAdjuster(19.3),
                new PercentGarnishmentRollupAdjuster(24.5),
            },
            new List<string>(),
            0.2,
            23826.6);
    }

    public double Price(GarnishmentRollup item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
