namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat segment calculation.</summary>
public interface ILicenseSeatSegmentStep
{
    string Describe();
    double Adjust(LicenseSeatSegment item, double valueValue);
}

public sealed record RoundingLicenseSeatSegmentAdjuster(double Step) : ILicenseSeatSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelLicenseSeatSegmentAdjuster(Dictionary<string, double> Factors) : ILicenseSeatSegmentStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LicenseSeatSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLicenseSeatSegmentAdjuster(double Cap) : ILicenseSeatSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LicenseSeatSegmentHoursOffsetAdjuster(double Weight) : ILicenseSeatSegmentStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LicenseSeatSegment item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the license seat segment steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatSegmentEngine(List<ILicenseSeatSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatSegmentEngine DefaultLicenseSeatSegmentEngine()
    {
        return new LicenseSeatSegmentEngine(
            new List<ILicenseSeatSegmentStep>
            {
                new RoundingLicenseSeatSegmentAdjuster(5.0),
                new LabelLicenseSeatSegmentAdjuster(new Dictionary<string, double> { ["south"] = 0.99 }),
                new CapLicenseSeatSegmentAdjuster(46796.0),
                new LicenseSeatSegmentHoursOffsetAdjuster(0.45),
            },
            new List<string>(),
            0.7,
            49766.9);
    }

    public double Compute(LicenseSeatSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
