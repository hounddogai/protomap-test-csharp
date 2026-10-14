namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg rollup calculation.</summary>
public interface IReorgRollupModifier
{
    string Describe();
    double Adjust(ReorgRollup item, double valueValue);
}

public sealed record MinimumReorgRollupAdjuster(double Minimum) : IReorgRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapReorgRollupAdjuster(double Cap) : IReorgRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingReorgRollupAdjuster(double Step) : IReorgRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedReorgRollupAdjuster(Registry Flags, string Flag, double Percent) : IReorgRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg rollup steps in order and keeps a trace of each.</summary>
public sealed class ReorgRollupEngine(List<IReorgRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgRollupEngine DefaultReorgRollupEngine(Registry flags)
    {
        return new ReorgRollupEngine(
            new List<IReorgRollupModifier>
            {
                new MinimumReorgRollupAdjuster(12.6),
                new CapReorgRollupAdjuster(40905.0),
                new RoundingReorgRollupAdjuster(0.1),
                new FlaggedReorgRollupAdjuster(flags, "reorg-rollup-default", -4.1),
            },
            new List<string>(),
            4.0,
            50953.1);
    }

    public double Evaluate(ReorgRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
