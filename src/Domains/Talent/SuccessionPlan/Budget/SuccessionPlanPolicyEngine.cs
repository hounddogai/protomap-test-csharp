namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>One step of the succession plan policy calculation.</summary>
public interface ISuccessionPlanPolicyModifier
{
    string Describe();
    double Adjust(SuccessionPlanPolicy item, double valueValue);
}

public sealed record SuccessionPlanPolicyEndsAtOffsetAdjuster(double Weight) : ISuccessionPlanPolicyModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(SuccessionPlanPolicy item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumSuccessionPlanPolicyAdjuster(double Minimum) : ISuccessionPlanPolicyModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSuccessionPlanPolicyAdjuster(double Cap) : ISuccessionPlanPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the succession plan policy steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanPolicyEngine(List<ISuccessionPlanPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanPolicyEngine DefaultSuccessionPlanPolicyEngine()
    {
        return new SuccessionPlanPolicyEngine(
            new List<ISuccessionPlanPolicyModifier>
            {
                new SuccessionPlanPolicyEndsAtOffsetAdjuster(0.83),
                new MinimumSuccessionPlanPolicyAdjuster(5.6),
                new CapSuccessionPlanPolicyAdjuster(19453.0),
            },
            new List<string>(),
            4.2,
            53072.6);
    }

    public double Run(SuccessionPlanPolicy item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
