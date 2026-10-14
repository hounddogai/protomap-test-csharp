namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor policy calculation.</summary>
public interface IVisitorPolicyAdjuster
{
    string Describe();
    double Adjust(VisitorPolicy item, double valueValue);
}

public sealed record CapVisitorPolicyAdjuster(double Cap) : IVisitorPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryVisitorPolicyAdjuster(Dictionary<string, double> Factors) : IVisitorPolicyAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(VisitorPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedVisitorPolicyAdjuster(Registry Flags, string Flag, double Percent) : IVisitorPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VisitorPolicyCreatedAtOffsetAdjuster(double Weight) : IVisitorPolicyAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(VisitorPolicy item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the visitor policy steps in order and keeps a trace of each.</summary>
public sealed class VisitorPolicyEngine(List<IVisitorPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorPolicyEngine DefaultVisitorPolicyEngine(Registry flags)
    {
        return new VisitorPolicyEngine(
            new List<IVisitorPolicyAdjuster>
            {
                new CapVisitorPolicyAdjuster(43807.0),
                new CountryVisitorPolicyAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.95, ["retail"] = 0.87, ["central"] = 1.26 }),
                new FlaggedVisitorPolicyAdjuster(flags, "visitor-policy-legacy", -9.2),
                new VisitorPolicyCreatedAtOffsetAdjuster(1.61),
            },
            new List<string>(),
            2.1,
            25053.0);
    }

    public double Evaluate(VisitorPolicy item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
