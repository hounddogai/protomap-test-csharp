namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge score calculation.</summary>
public interface IAccessBadgeScoreStep
{
    string Describe();
    double Adjust(AccessBadgeScore item, double valueValue);
}

public sealed record CapAccessBadgeScoreAdjuster(double Cap) : IAccessBadgeScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAccessBadgeScoreAdjuster(double Minimum) : IAccessBadgeScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeScoreAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeScore item, double valueValue)
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

public sealed record RoundingAccessBadgeScoreAdjuster(double Step) : IAccessBadgeScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameAccessBadgeScoreAdjuster(Dictionary<string, double> Factors) : IAccessBadgeScoreStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccessBadgeScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the access badge score steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeScoreEngine(List<IAccessBadgeScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeScoreEngine DefaultAccessBadgeScoreEngine()
    {
        return new AccessBadgeScoreEngine(
            new List<IAccessBadgeScoreStep>
            {
                new CapAccessBadgeScoreAdjuster(28667.0),
                new MinimumAccessBadgeScoreAdjuster(14.6),
                new TieredAccessBadgeScoreAdjuster(
                    new List<double> { 2714.0, 7903.0, 9626.0 },
                    new List<double> { 1.0, 1.01, 1.06 }),
                new RoundingAccessBadgeScoreAdjuster(0.1),
                new NameAccessBadgeScoreAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["south"] = 1.15 }),
            },
            new List<string>(),
            0.0,
            74917.7);
    }

    public double Evaluate(AccessBadgeScore item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
