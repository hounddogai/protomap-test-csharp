namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment summary calculation.</summary>
public interface IGarnishmentSummaryAdjuster
{
    string Describe();
    double Adjust(GarnishmentSummary item, double valueValue);
}

public sealed record FlaggedGarnishmentSummaryAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentSummaryAdjuster(double Cap) : IGarnishmentSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGarnishmentSummaryAdjuster(double Step) : IGarnishmentSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGarnishmentSummaryAdjuster(double Minimum) : IGarnishmentSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the garnishment summary steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentSummaryEngine(List<IGarnishmentSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentSummaryEngine DefaultGarnishmentSummaryEngine(Registry flags)
    {
        return new GarnishmentSummaryEngine(
            new List<IGarnishmentSummaryAdjuster>
            {
                new FlaggedGarnishmentSummaryAdjuster(flags, "garnishment-summary-seasonal", -7.9),
                new CapGarnishmentSummaryAdjuster(10208.0),
                new RoundingGarnishmentSummaryAdjuster(0.1),
                new MinimumGarnishmentSummaryAdjuster(28.9),
            },
            new List<string>(),
            4.2,
            7820.8);
    }

    public double Price(GarnishmentSummary item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
