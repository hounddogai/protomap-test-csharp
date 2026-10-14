namespace Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>One step of the garnishment segment calculation.</summary>
public interface IGarnishmentSegmentFactor
{
    string Describe();
    double Adjust(GarnishmentSegment item, double valueValue);
}

public sealed record MinimumGarnishmentSegmentAdjuster(double Minimum) : IGarnishmentSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GarnishmentSegmentRateOffsetAdjuster(double Weight) : IGarnishmentSegmentFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(GarnishmentSegment item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record PercentGarnishmentSegmentAdjuster(double Percent) : IGarnishmentSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentGarnishmentSegmentAdjuster(Dictionary<string, double> Factors) : IGarnishmentSegmentFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(GarnishmentSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the garnishment segment steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentSegmentEngine(List<IGarnishmentSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentSegmentEngine DefaultGarnishmentSegmentEngine()
    {
        return new GarnishmentSegmentEngine(
            new List<IGarnishmentSegmentFactor>
            {
                new MinimumGarnishmentSegmentAdjuster(3.2),
                new GarnishmentSegmentRateOffsetAdjuster(1.48),
                new PercentGarnishmentSegmentAdjuster(21.3),
                new DepartmentGarnishmentSegmentAdjuster(new Dictionary<string, double> { ["north"] = 1.05 }),
            },
            new List<string>(),
            3.6,
            75134.8);
    }

    public double Price(GarnishmentSegment item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
