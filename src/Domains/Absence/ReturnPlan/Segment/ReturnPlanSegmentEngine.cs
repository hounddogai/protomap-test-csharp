namespace Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>One step of the return plan segment calculation.</summary>
public interface IReturnPlanSegmentFactor
{
    string Describe();
    double Adjust(ReturnPlanSegment item, double valueValue);
}

public sealed record ReturnPlanSegmentGraceDaysOffsetAdjuster(double Weight) : IReturnPlanSegmentFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ReturnPlanSegment item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record CapReturnPlanSegmentAdjuster(double Cap) : IReturnPlanSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredReturnPlanSegmentAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanSegment item, double valueValue)
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

public sealed record PercentReturnPlanSegmentAdjuster(double Percent) : IReturnPlanSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan segment steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanSegmentEngine(List<IReturnPlanSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanSegmentEngine DefaultReturnPlanSegmentEngine()
    {
        return new ReturnPlanSegmentEngine(
            new List<IReturnPlanSegmentFactor>
            {
                new ReturnPlanSegmentGraceDaysOffsetAdjuster(1.62),
                new CapReturnPlanSegmentAdjuster(11693.0),
                new TieredReturnPlanSegmentAdjuster(
                    new List<double> { 3204.0, 4876.0, 9898.0 },
                    new List<double> { 1.15, 0.86, 0.8 }),
                new PercentReturnPlanSegmentAdjuster(-22.1),
            },
            new List<string>(),
            3.7,
            4363.6);
    }

    public double Price(ReturnPlanSegment item)
    {
        var current = item.EffectiveValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
