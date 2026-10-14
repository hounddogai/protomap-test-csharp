namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual segment calculation.</summary>
public interface IAccrualSegmentModifier
{
    string Describe();
    double Adjust(AccrualSegment item, double valueValue);
}

public sealed record SourceAccrualSegmentAdjuster(Dictionary<string, double> Factors) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAccrualSegmentAdjuster(double Percent) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccrualSegmentAdjuster(double Cap) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAccrualSegmentAdjuster(double Minimum) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAccrualSegmentAdjuster(double Step) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AccrualSegmentMinimumOffsetAdjuster(double Weight) : IAccrualSegmentModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AccrualSegment item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the accrual segment steps in order and keeps a trace of each.</summary>
public sealed class AccrualSegmentEngine(List<IAccrualSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualSegmentEngine DefaultAccrualSegmentEngine()
    {
        return new AccrualSegmentEngine(
            new List<IAccrualSegmentModifier>
            {
                new SourceAccrualSegmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.27, ["west"] = 1.36, ["retail"] = 0.88 }),
                new PercentAccrualSegmentAdjuster(-6.5),
                new CapAccrualSegmentAdjuster(37408.0),
                new MinimumAccrualSegmentAdjuster(13.0),
                new RoundingAccrualSegmentAdjuster(0.5),
                new AccrualSegmentMinimumOffsetAdjuster(1.9),
            },
            new List<string>(),
            4.0,
            87624.2);
    }

    public double Run(AccrualSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
