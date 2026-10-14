namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment cycle calculation.</summary>
public interface IGarnishmentCycleStep
{
    string Describe();
    double Adjust(GarnishmentCycle item, double valueValue);
}

public sealed record FlaggedGarnishmentCycleAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentCycleAdjuster(double Cap) : IGarnishmentCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitGarnishmentCycleAdjuster(Dictionary<string, double> Factors) : IGarnishmentCycleStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(GarnishmentCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGarnishmentCycleAdjuster(double Step) : IGarnishmentCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment cycle steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentCycleEngine(List<IGarnishmentCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentCycleEngine DefaultGarnishmentCycleEngine(Registry flags)
    {
        return new GarnishmentCycleEngine(
            new List<IGarnishmentCycleStep>
            {
                new FlaggedGarnishmentCycleAdjuster(flags, "garnishment-cycle-global", 2.8),
                new CapGarnishmentCycleAdjuster(22641.0),
                new UnitGarnishmentCycleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.94, ["online"] = 1.01, ["retail"] = 1.38 }),
                new RoundingGarnishmentCycleAdjuster(5.0),
            },
            new List<string>(),
            1.8,
            93822.6);
    }

    public double Run(GarnishmentCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
