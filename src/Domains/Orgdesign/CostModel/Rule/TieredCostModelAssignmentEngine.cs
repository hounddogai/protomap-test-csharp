namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered cost model assignment calculation.</summary>
public interface ITieredCostModelAssignmentModifier
{
    string Describe();
    double Adjust(TieredCostModelAssignment item, double valueValue);
}

public sealed record FlaggedTieredCostModelAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ITieredCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredCostModelAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTieredCostModelAssignmentAdjuster(double Percent) : ITieredCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCostModelAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredCostModelAssignmentAdjuster(double Minimum) : ITieredCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredCostModelAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCostModelAssignmentEndsAtOffsetAdjuster(double Weight) : ITieredCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(TieredCostModelAssignment item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the tiered cost model assignment steps in order and keeps a trace of each.</summary>
public sealed class TieredCostModelAssignmentEngine(List<ITieredCostModelAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCostModelAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCostModelAssignmentEngine DefaultTieredCostModelAssignmentEngine(Registry flags)
    {
        return new TieredCostModelAssignmentEngine(
            new List<ITieredCostModelAssignmentModifier>
            {
                new FlaggedTieredCostModelAssignmentAdjuster(flags, "tiered-cost-model-assignment-adaptive", 7.0),
                new PercentTieredCostModelAssignmentAdjuster(8.8),
                new MinimumTieredCostModelAssignmentAdjuster(17.8),
                new TieredCostModelAssignmentEndsAtOffsetAdjuster(0.22),
            },
            new List<string>(),
            3.3,
            68253.2);
    }

    public double Evaluate(TieredCostModelAssignment item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCostModelAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
