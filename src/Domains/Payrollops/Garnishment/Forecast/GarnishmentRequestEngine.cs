namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment request calculation.</summary>
public interface IGarnishmentRequestAdjuster
{
    string Describe();
    double Adjust(GarnishmentRequest item, double valueValue);
}

public sealed record TitleGarnishmentRequestAdjuster(Dictionary<string, double> Factors) : IGarnishmentRequestAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GarnishmentRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGarnishmentRequestAdjuster(double Percent) : IGarnishmentRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentRequestAdjuster(double Cap) : IGarnishmentRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGarnishmentRequestAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment request steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentRequestEngine(List<IGarnishmentRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentRequestEngine DefaultGarnishmentRequestEngine(Registry flags)
    {
        return new GarnishmentRequestEngine(
            new List<IGarnishmentRequestAdjuster>
            {
                new TitleGarnishmentRequestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["central"] = 0.96, ["south"] = 0.86 }),
                new PercentGarnishmentRequestAdjuster(6.8),
                new CapGarnishmentRequestAdjuster(24162.0),
                new FlaggedGarnishmentRequestAdjuster(flags, "garnishment-request-adaptive", -8.1),
            },
            new List<string>(),
            2.8,
            93439.3);
    }

    public double Evaluate(GarnishmentRequest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
