namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span budget calculation.</summary>
public interface ISpanBudgetModifier
{
    string Describe();
    double Adjust(SpanBudget item, double valueValue);
}

public sealed record RoundingSpanBudgetAdjuster(double Step) : ISpanBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSpanBudgetAdjuster(double Percent) : ISpanBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanBudgetAdjuster(double Minimum) : ISpanBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the span budget steps in order and keeps a trace of each.</summary>
public sealed class SpanBudgetEngine(List<ISpanBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanBudgetEngine DefaultSpanBudgetEngine()
    {
        return new SpanBudgetEngine(
            new List<ISpanBudgetModifier>
            {
                new RoundingSpanBudgetAdjuster(0.5),
                new PercentSpanBudgetAdjuster(-5.3),
                new MinimumSpanBudgetAdjuster(13.4),
            },
            new List<string>(),
            3.9,
            61420.6);
    }

    public double Compute(SpanBudget item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
