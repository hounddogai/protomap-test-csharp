namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment group calculation.</summary>
public interface IGarnishmentGroupModifier
{
    string Describe();
    double Adjust(GarnishmentGroup item, double valueValue);
}

public sealed record GarnishmentGroupScoreOffsetAdjuster(double Weight) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record UnitGarnishmentGroupAdjuster(Dictionary<string, double> Factors) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGarnishmentGroupAdjuster(double Cap) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGarnishmentGroupAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGarnishmentGroupAdjuster(double Minimum) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGarnishmentGroupAdjuster(double Step) : IGarnishmentGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment group steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentGroupEngine(List<IGarnishmentGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentGroupEngine DefaultGarnishmentGroupEngine(Registry flags)
    {
        return new GarnishmentGroupEngine(
            new List<IGarnishmentGroupModifier>
            {
                new GarnishmentGroupScoreOffsetAdjuster(1.75),
                new UnitGarnishmentGroupAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["retail"] = 1.35, ["south"] = 0.95 }),
                new CapGarnishmentGroupAdjuster(41209.0),
                new FlaggedGarnishmentGroupAdjuster(flags, "garnishment-group-capped", 5.9),
                new MinimumGarnishmentGroupAdjuster(29.2),
                new RoundingGarnishmentGroupAdjuster(0.5),
            },
            new List<string>(),
            2.9,
            24088.8);
    }

    public double Compute(GarnishmentGroup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
