namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment policy calculation.</summary>
public interface IAssignmentPolicyFactor
{
    string Describe();
    double Adjust(AssignmentPolicy item, double valueValue);
}

public sealed record FlaggedAssignmentPolicyAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AssignmentPolicyMarginOffsetAdjuster(double Weight) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredAssignmentPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
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

public sealed record PercentAssignmentPolicyAdjuster(double Percent) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAssignmentPolicyAdjuster(double Cap) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAssignmentPolicyAdjuster(double Step) : IAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the assignment policy steps in order and keeps a trace of each.</summary>
public sealed class AssignmentPolicyEngine(List<IAssignmentPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentPolicyEngine DefaultAssignmentPolicyEngine(Registry flags)
    {
        return new AssignmentPolicyEngine(
            new List<IAssignmentPolicyFactor>
            {
                new FlaggedAssignmentPolicyAdjuster(flags, "assignment-policy-manual", -7.3),
                new AssignmentPolicyMarginOffsetAdjuster(0.49),
                new TieredAssignmentPolicyAdjuster(
                    new List<double> { 361.0, 4332.0, 9631.0 },
                    new List<double> { 0.86, 1.14, 0.99 }),
                new PercentAssignmentPolicyAdjuster(16.2),
                new CapAssignmentPolicyAdjuster(20361.0),
                new RoundingAssignmentPolicyAdjuster(1.0),
            },
            new List<string>(),
            1.0,
            92271.1);
    }

    public double Compute(AssignmentPolicy item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
