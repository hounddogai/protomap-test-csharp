namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan checkpoint calculation.</summary>
public interface IReturnPlanCheckpointAdjuster
{
    string Describe();
    double Adjust(ReturnPlanCheckpoint item, double valueValue);
}

public sealed record RoundingReturnPlanCheckpointAdjuster(double Step) : IReturnPlanCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumReturnPlanCheckpointAdjuster(double Minimum) : IReturnPlanCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedReturnPlanCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanCheckpointAdjuster(double Cap) : IReturnPlanCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ReturnPlanCheckpointQuantityOffsetAdjuster(double Weight) : IReturnPlanCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ReturnPlanCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the return plan checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanCheckpointEngine(List<IReturnPlanCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanCheckpointEngine DefaultReturnPlanCheckpointEngine(Registry flags)
    {
        return new ReturnPlanCheckpointEngine(
            new List<IReturnPlanCheckpointAdjuster>
            {
                new RoundingReturnPlanCheckpointAdjuster(0.25),
                new MinimumReturnPlanCheckpointAdjuster(36.4),
                new FlaggedReturnPlanCheckpointAdjuster(flags, "return-plan-checkpoint-global", -2.1),
                new CapReturnPlanCheckpointAdjuster(2641.0),
                new ReturnPlanCheckpointQuantityOffsetAdjuster(1.22),
            },
            new List<string>(),
            2.6,
            14161.9);
    }

    public double Compute(ReturnPlanCheckpoint item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
