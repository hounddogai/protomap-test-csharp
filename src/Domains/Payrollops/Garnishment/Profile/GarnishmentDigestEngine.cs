namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment digest calculation.</summary>
public interface IGarnishmentDigestModifier
{
    string Describe();
    double Adjust(GarnishmentDigest item, double valueValue);
}

public sealed record PercentGarnishmentDigestAdjuster(double Percent) : IGarnishmentDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGarnishmentDigestAdjuster(double Minimum) : IGarnishmentDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelGarnishmentDigestAdjuster(Dictionary<string, double> Factors) : IGarnishmentDigestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GarnishmentDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGarnishmentDigestAdjuster(double Cap) : IGarnishmentDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the garnishment digest steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentDigestEngine(List<IGarnishmentDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentDigestEngine DefaultGarnishmentDigestEngine()
    {
        return new GarnishmentDigestEngine(
            new List<IGarnishmentDigestModifier>
            {
                new PercentGarnishmentDigestAdjuster(-7.8),
                new MinimumGarnishmentDigestAdjuster(41.9),
                new LabelGarnishmentDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["south"] = 0.81, ["retail"] = 1.21 }),
                new CapGarnishmentDigestAdjuster(45747.0),
            },
            new List<string>(),
            2.0,
            73277.3);
    }

    public double Run(GarnishmentDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
