namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant summary calculation.</summary>
public interface IEquityGrantSummaryStep
{
    string Describe();
    double Adjust(EquityGrantSummary item, double valueValue);
}

public sealed record TieredEquityGrantSummaryAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantSummary item, double valueValue)
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

public sealed record FlaggedEquityGrantSummaryAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EquityGrantSummaryAmountOffsetAdjuster(double Weight) : IEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(EquityGrantSummary item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumEquityGrantSummaryAdjuster(double Minimum) : IEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapEquityGrantSummaryAdjuster(double Cap) : IEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the equity grant summary steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantSummaryEngine(List<IEquityGrantSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantSummaryEngine DefaultEquityGrantSummaryEngine(Registry flags)
    {
        return new EquityGrantSummaryEngine(
            new List<IEquityGrantSummaryStep>
            {
                new TieredEquityGrantSummaryAdjuster(
                    new List<double> { 278.0, 2012.0, 5983.0 },
                    new List<double> { 0.84, 1.04, 0.87 }),
                new FlaggedEquityGrantSummaryAdjuster(flags, "equity-grant-summary-seasonal", 9.9),
                new EquityGrantSummaryAmountOffsetAdjuster(1.93),
                new MinimumEquityGrantSummaryAdjuster(46.2),
                new CapEquityGrantSummaryAdjuster(49746.0),
            },
            new List<string>(),
            1.4,
            14030.7);
    }

    public double Evaluate(EquityGrantSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
