namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor summary calculation.</summary>
public interface IVisitorSummaryStep
{
    string Describe();
    double Adjust(VisitorSummary item, double valueValue);
}

public sealed record CapVisitorSummaryAdjuster(double Cap) : IVisitorSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VisitorSummaryQuantityOffsetAdjuster(double Weight) : IVisitorSummaryStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record SourceVisitorSummaryAdjuster(Dictionary<string, double> Factors) : IVisitorSummaryStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedVisitorSummaryAdjuster(Registry Flags, string Flag, double Percent) : IVisitorSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentVisitorSummaryAdjuster(double Percent) : IVisitorSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVisitorSummaryAdjuster(double Step) : IVisitorSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor summary steps in order and keeps a trace of each.</summary>
public sealed class VisitorSummaryEngine(List<IVisitorSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorSummaryEngine DefaultVisitorSummaryEngine(Registry flags)
    {
        return new VisitorSummaryEngine(
            new List<IVisitorSummaryStep>
            {
                new CapVisitorSummaryAdjuster(24324.0),
                new VisitorSummaryQuantityOffsetAdjuster(1.5),
                new SourceVisitorSummaryAdjuster(new Dictionary<string, double> { ["online"] = 1.35 }),
                new FlaggedVisitorSummaryAdjuster(flags, "visitor-summary-rolling", 3.2),
                new PercentVisitorSummaryAdjuster(16.3),
                new RoundingVisitorSummaryAdjuster(0.05),
            },
            new List<string>(),
            1.5,
            21059.3);
    }

    public double Evaluate(VisitorSummary item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
