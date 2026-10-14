namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment mapping calculation.</summary>
public interface IGarnishmentMappingStep
{
    string Describe();
    double Adjust(GarnishmentMapping item, double valueValue);
}

public sealed record MinimumGarnishmentMappingAdjuster(double Minimum) : IGarnishmentMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGarnishmentMappingAdjuster(double Cap) : IGarnishmentMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGarnishmentMappingAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryGarnishmentMappingAdjuster(Dictionary<string, double> Factors) : IGarnishmentMappingStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GarnishmentMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGarnishmentMappingAdjuster(double Percent) : IGarnishmentMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment mapping steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentMappingEngine(List<IGarnishmentMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentMappingEngine DefaultGarnishmentMappingEngine(Registry flags)
    {
        return new GarnishmentMappingEngine(
            new List<IGarnishmentMappingStep>
            {
                new MinimumGarnishmentMappingAdjuster(38.5),
                new CapGarnishmentMappingAdjuster(46127.0),
                new FlaggedGarnishmentMappingAdjuster(flags, "garnishment-mapping-pending", -4.6),
                new CountryGarnishmentMappingAdjuster(new Dictionary<string, double> { ["north"] = 1.27 }),
                new PercentGarnishmentMappingAdjuster(12.8),
            },
            new List<string>(),
            3.1,
            27363.6);
    }

    public double Evaluate(GarnishmentMapping item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
