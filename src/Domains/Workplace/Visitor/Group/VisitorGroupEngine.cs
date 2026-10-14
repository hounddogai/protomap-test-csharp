namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor group calculation.</summary>
public interface IVisitorGroupAdjuster
{
    string Describe();
    double Adjust(VisitorGroup item, double valueValue);
}

public sealed record TieredVisitorGroupAdjuster(List<double> Bounds, List<double> Rates) : IVisitorGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorGroup item, double valueValue)
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

public sealed record RoundingVisitorGroupAdjuster(double Step) : IVisitorGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentVisitorGroupAdjuster(double Percent) : IVisitorGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVisitorGroupAdjuster(double Cap) : IVisitorGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the visitor group steps in order and keeps a trace of each.</summary>
public sealed class VisitorGroupEngine(List<IVisitorGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorGroupEngine DefaultVisitorGroupEngine()
    {
        return new VisitorGroupEngine(
            new List<IVisitorGroupAdjuster>
            {
                new TieredVisitorGroupAdjuster(new List<double> { 6965.0, 8914.0 }, new List<double> { 0.87, 0.94 }),
                new RoundingVisitorGroupAdjuster(1.0),
                new PercentVisitorGroupAdjuster(-19.7),
                new CapVisitorGroupAdjuster(39029.0),
            },
            new List<string>(),
            4.5,
            2978.7);
    }

    public double Evaluate(VisitorGroup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
