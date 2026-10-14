namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception budget calculation.</summary>
public interface IExceptionBudgetFactor
{
    string Describe();
    double Adjust(ExceptionBudget item, double valueValue);
}

public sealed record MinimumExceptionBudgetAdjuster(double Minimum) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapExceptionBudgetAdjuster(double Cap) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentExceptionBudgetAdjuster(Dictionary<string, double> Factors) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredExceptionBudgetAdjuster(List<double> Bounds, List<double> Rates) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
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

public sealed record PercentExceptionBudgetAdjuster(double Percent) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingExceptionBudgetAdjuster(double Step) : IExceptionBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the exception budget steps in order and keeps a trace of each.</summary>
public sealed class ExceptionBudgetEngine(List<IExceptionBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionBudgetEngine DefaultExceptionBudgetEngine()
    {
        return new ExceptionBudgetEngine(
            new List<IExceptionBudgetFactor>
            {
                new MinimumExceptionBudgetAdjuster(7.3),
                new CapExceptionBudgetAdjuster(8644.0),
                new SegmentExceptionBudgetAdjuster(new Dictionary<string, double> { ["central"] = 0.93 }),
                new TieredExceptionBudgetAdjuster(
                    new List<double> { 467.0, 1815.0, 2215.0 },
                    new List<double> { 0.96, 0.81, 1.1 }),
                new PercentExceptionBudgetAdjuster(-24.0),
                new RoundingExceptionBudgetAdjuster(0.1),
            },
            new List<string>(),
            2.4,
            62238.0);
    }

    public double Evaluate(ExceptionBudget item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
