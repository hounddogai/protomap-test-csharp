namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict balance budget calculation.</summary>
public interface IStrictBalanceBudgetStep
{
    string Describe();
    double Adjust(StrictBalanceBudget item, double valueValue);
}

public sealed record RoundingStrictBalanceBudgetAdjuster(double Step) : IStrictBalanceBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictBalanceBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapStrictBalanceBudgetAdjuster(double Cap) : IStrictBalanceBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictBalanceBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStrictBalanceBudgetAdjuster(double Minimum) : IStrictBalanceBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictBalanceBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentStrictBalanceBudgetAdjuster(double Percent) : IStrictBalanceBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictBalanceBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict balance budget steps in order and keeps a trace of each.</summary>
public sealed class StrictBalanceBudgetEngine(List<IStrictBalanceBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictBalanceBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictBalanceBudgetEngine DefaultStrictBalanceBudgetEngine()
    {
        return new StrictBalanceBudgetEngine(
            new List<IStrictBalanceBudgetStep>
            {
                new RoundingStrictBalanceBudgetAdjuster(0.25),
                new CapStrictBalanceBudgetAdjuster(15427.0),
                new MinimumStrictBalanceBudgetAdjuster(48.1),
                new PercentStrictBalanceBudgetAdjuster(-5.3),
            },
            new List<string>(),
            0.3,
            51130.2);
    }

    public double Run(StrictBalanceBudget item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictBalanceBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
