namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional cost model assignment calculation.</summary>
public interface IRegionalCostModelAssignmentModifier
{
    string Describe();
    double Adjust(RegionalCostModelAssignment item, double valueValue);
}

public sealed record FlaggedRegionalCostModelAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IRegionalCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalCostModelAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalCostModelAssignmentAdjuster(double Minimum) : IRegionalCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalCostModelAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRegionalCostModelAssignmentAdjuster(Dictionary<string, double> Factors) : IRegionalCostModelAssignmentModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalCostModelAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalCostModelAssignmentAdjuster(double Percent) : IRegionalCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalCostModelAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalCostModelAssignmentAdjuster(double Step) : IRegionalCostModelAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalCostModelAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional cost model assignment steps in order and keeps a trace of each.</summary>
public sealed class RegionalCostModelAssignmentEngine(List<IRegionalCostModelAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalCostModelAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalCostModelAssignmentEngine DefaultRegionalCostModelAssignmentEngine(Registry flags)
    {
        return new RegionalCostModelAssignmentEngine(
            new List<IRegionalCostModelAssignmentModifier>
            {
                new FlaggedRegionalCostModelAssignmentAdjuster(
                    flags,
                    "regional-cost-model-assignment-regional",
                    -2.5),
                new MinimumRegionalCostModelAssignmentAdjuster(40.3),
                new NameRegionalCostModelAssignmentAdjuster(new Dictionary<string, double> { ["west"] = 1.2 }),
                new PercentRegionalCostModelAssignmentAdjuster(2.9),
                new RoundingRegionalCostModelAssignmentAdjuster(0.5),
            },
            new List<string>(),
            1.8,
            50333.1);
    }

    public double Run(RegionalCostModelAssignment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalCostModelAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
