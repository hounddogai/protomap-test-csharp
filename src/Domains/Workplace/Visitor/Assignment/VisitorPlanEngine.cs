namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor plan calculation.</summary>
public interface IVisitorPlanStep
{
    string Describe();
    double Adjust(VisitorPlan item, double valueValue);
}

public sealed record VisitorPlanBudgetOffsetAdjuster(double Weight) : IVisitorPlanStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(VisitorPlan item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapVisitorPlanAdjuster(double Cap) : IVisitorPlanStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumVisitorPlanAdjuster(double Minimum) : IVisitorPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceVisitorPlanAdjuster(Dictionary<string, double> Factors) : IVisitorPlanStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(VisitorPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentVisitorPlanAdjuster(double Percent) : IVisitorPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor plan steps in order and keeps a trace of each.</summary>
public sealed class VisitorPlanEngine(List<IVisitorPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorPlanEngine DefaultVisitorPlanEngine()
    {
        return new VisitorPlanEngine(
            new List<IVisitorPlanStep>
            {
                new VisitorPlanBudgetOffsetAdjuster(1.23),
                new CapVisitorPlanAdjuster(43093.0),
                new MinimumVisitorPlanAdjuster(41.4),
                new SourceVisitorPlanAdjuster(new Dictionary<string, double> { ["online"] = 0.81 }),
                new PercentVisitorPlanAdjuster(25.0),
            },
            new List<string>(),
            3.5,
            45100.2);
    }

    public double Run(VisitorPlan item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
