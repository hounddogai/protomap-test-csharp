namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor score calculation.</summary>
public interface IVisitorScoreModifier
{
    string Describe();
    double Adjust(VisitorScore item, double valueValue);
}

public sealed record CapVisitorScoreAdjuster(double Cap) : IVisitorScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumVisitorScoreAdjuster(double Minimum) : IVisitorScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingVisitorScoreAdjuster(double Step) : IVisitorScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record VisitorScoreRateOffsetAdjuster(double Weight) : IVisitorScoreModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(VisitorScore item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record NameVisitorScoreAdjuster(Dictionary<string, double> Factors) : IVisitorScoreModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(VisitorScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredVisitorScoreAdjuster(List<double> Bounds, List<double> Rates) : IVisitorScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorScore item, double valueValue)
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

/// <summary>Runs the visitor score steps in order and keeps a trace of each.</summary>
public sealed class VisitorScoreEngine(List<IVisitorScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorScoreEngine DefaultVisitorScoreEngine()
    {
        return new VisitorScoreEngine(
            new List<IVisitorScoreModifier>
            {
                new CapVisitorScoreAdjuster(34835.0),
                new MinimumVisitorScoreAdjuster(50.0),
                new RoundingVisitorScoreAdjuster(0.1),
                new VisitorScoreRateOffsetAdjuster(0.71),
                new NameVisitorScoreAdjuster(new Dictionary<string, double> { ["retail"] = 1.3, ["north"] = 0.8 }),
                new TieredVisitorScoreAdjuster(
                    new List<double> { 2680.0, 4451.0, 9823.0 },
                    new List<double> { 1.1, 0.92, 0.93 }),
            },
            new List<string>(),
            2.1,
            52682.1);
    }

    public double Price(VisitorScore item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
