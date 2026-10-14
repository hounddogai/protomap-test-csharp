namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge budget calculation.</summary>
public interface IAccessBadgeBudgetModifier
{
    string Describe();
    double Adjust(AccessBadgeBudget item, double valueValue);
}

public sealed record AccessBadgeBudgetRevisionOffsetAdjuster(double Weight) : IAccessBadgeBudgetModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AccessBadgeBudget item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentAccessBadgeBudgetAdjuster(double Percent) : IAccessBadgeBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccessBadgeBudgetAdjuster(double Cap) : IAccessBadgeBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the access badge budget steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeBudgetEngine(List<IAccessBadgeBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeBudgetEngine DefaultAccessBadgeBudgetEngine()
    {
        return new AccessBadgeBudgetEngine(
            new List<IAccessBadgeBudgetModifier>
            {
                new AccessBadgeBudgetRevisionOffsetAdjuster(1.7),
                new PercentAccessBadgeBudgetAdjuster(-5.4),
                new CapAccessBadgeBudgetAdjuster(39712.0),
            },
            new List<string>(),
            3.7,
            44003.9);
    }

    public double Evaluate(AccessBadgeBudget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
