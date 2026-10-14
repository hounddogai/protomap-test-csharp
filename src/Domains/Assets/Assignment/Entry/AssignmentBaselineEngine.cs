namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment baseline calculation.</summary>
public interface IAssignmentBaselineAdjuster
{
    string Describe();
    double Adjust(AssignmentBaseline item, double valueValue);
}

public sealed record RoundingAssignmentBaselineAdjuster(double Step) : IAssignmentBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAssignmentBaselineAdjuster(double Percent) : IAssignmentBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssignmentBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentBaseline item, double valueValue)
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

public sealed record CapAssignmentBaselineAdjuster(double Cap) : IAssignmentBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAssignmentBaselineAdjuster(double Minimum) : IAssignmentBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the assignment baseline steps in order and keeps a trace of each.</summary>
public sealed class AssignmentBaselineEngine(List<IAssignmentBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentBaselineEngine DefaultAssignmentBaselineEngine()
    {
        return new AssignmentBaselineEngine(
            new List<IAssignmentBaselineAdjuster>
            {
                new RoundingAssignmentBaselineAdjuster(5.0),
                new PercentAssignmentBaselineAdjuster(-23.0),
                new TieredAssignmentBaselineAdjuster(
                    new List<double> { 1131.0, 3701.0, 6316.0, 7727.0 },
                    new List<double> { 1.06, 0.89, 0.97, 1.07 }),
                new CapAssignmentBaselineAdjuster(8496.0),
                new MinimumAssignmentBaselineAdjuster(13.3),
            },
            new List<string>(),
            1.8,
            49347.1);
    }

    public double Price(AssignmentBaseline item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
