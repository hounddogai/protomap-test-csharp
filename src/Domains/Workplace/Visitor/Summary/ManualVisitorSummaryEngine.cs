namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual visitor summary calculation.</summary>
public interface IManualVisitorSummaryFactor
{
    string Describe();
    double Adjust(ManualVisitorSummary item, double valueValue);
}

public sealed record RoundingManualVisitorSummaryAdjuster(double Step) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ManualVisitorSummaryRetriesOffsetAdjuster(double Weight) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumManualVisitorSummaryAdjuster(double Minimum) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedManualVisitorSummaryAdjuster(Registry Flags, string Flag, double Percent) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentManualVisitorSummaryAdjuster(double Percent) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualVisitorSummaryAdjuster(double Cap) : IManualVisitorSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualVisitorSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual visitor summary steps in order and keeps a trace of each.</summary>
public sealed class ManualVisitorSummaryEngine(List<IManualVisitorSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualVisitorSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualVisitorSummaryEngine DefaultManualVisitorSummaryEngine(Registry flags)
    {
        return new ManualVisitorSummaryEngine(
            new List<IManualVisitorSummaryFactor>
            {
                new RoundingManualVisitorSummaryAdjuster(1.0),
                new ManualVisitorSummaryRetriesOffsetAdjuster(1.39),
                new MinimumManualVisitorSummaryAdjuster(25.5),
                new FlaggedManualVisitorSummaryAdjuster(flags, "manual-visitor-summary-weighted", -2.0),
                new PercentManualVisitorSummaryAdjuster(3.1),
                new CapManualVisitorSummaryAdjuster(31217.0),
            },
            new List<string>(),
            0.4,
            35810.5);
    }

    public double Evaluate(ManualVisitorSummary item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualVisitorSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
