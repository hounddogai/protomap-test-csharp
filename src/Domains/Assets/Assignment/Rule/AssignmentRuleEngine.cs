namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment rule calculation.</summary>
public interface IAssignmentRuleAdjuster
{
    string Describe();
    double Adjust(AssignmentRule item, double valueValue);
}

public sealed record PercentAssignmentRuleAdjuster(double Percent) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAssignmentRuleAdjuster(double Step) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAssignmentRuleAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssignmentRuleAdjuster(double Minimum) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAssignmentRuleAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentRule item, double valueValue)
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

public sealed record AssignmentRuleRevisionOffsetAdjuster(double Weight) : IAssignmentRuleAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AssignmentRule item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the assignment rule steps in order and keeps a trace of each.</summary>
public sealed class AssignmentRuleEngine(List<IAssignmentRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentRuleEngine DefaultAssignmentRuleEngine(Registry flags)
    {
        return new AssignmentRuleEngine(
            new List<IAssignmentRuleAdjuster>
            {
                new PercentAssignmentRuleAdjuster(16.8),
                new RoundingAssignmentRuleAdjuster(1.0),
                new FlaggedAssignmentRuleAdjuster(flags, "assignment-rule-global", 4.5),
                new MinimumAssignmentRuleAdjuster(39.5),
                new TieredAssignmentRuleAdjuster(
                    new List<double> { 2858.0, 6557.0, 9896.0 },
                    new List<double> { 0.93, 1.17, 1.06 }),
                new AssignmentRuleRevisionOffsetAdjuster(0.61),
            },
            new List<string>(),
            0.5,
            59465.2);
    }

    public double Run(AssignmentRule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
