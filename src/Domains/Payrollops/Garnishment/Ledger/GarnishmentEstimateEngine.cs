namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment estimate calculation.</summary>
public interface IGarnishmentEstimateModifier
{
    string Describe();
    double Adjust(GarnishmentEstimate item, double valueValue);
}

public sealed record PercentGarnishmentEstimateAdjuster(double Percent) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGarnishmentEstimateAdjuster(double Minimum) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GarnishmentEstimateQuantityOffsetAdjuster(double Weight) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedGarnishmentEstimateAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentEstimateAdjuster(double Cap) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameGarnishmentEstimateAdjuster(Dictionary<string, double> Factors) : IGarnishmentEstimateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GarnishmentEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the garnishment estimate steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentEstimateEngine(List<IGarnishmentEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentEstimateEngine DefaultGarnishmentEstimateEngine(Registry flags)
    {
        return new GarnishmentEstimateEngine(
            new List<IGarnishmentEstimateModifier>
            {
                new PercentGarnishmentEstimateAdjuster(-6.3),
                new MinimumGarnishmentEstimateAdjuster(42.3),
                new GarnishmentEstimateQuantityOffsetAdjuster(0.86),
                new FlaggedGarnishmentEstimateAdjuster(flags, "garnishment-estimate-pending", -6.9),
                new CapGarnishmentEstimateAdjuster(22797.0),
                new NameGarnishmentEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.98, ["retail"] = 0.74 }),
            },
            new List<string>(),
            1.0,
            16215.3);
    }

    public double Compute(GarnishmentEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
