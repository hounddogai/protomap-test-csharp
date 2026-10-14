namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>One step of the lenient pay band bundle calculation.</summary>
public interface ILenientPayBandBundleStep
{
    string Describe();
    double Adjust(LenientPayBandBundle item, double valueValue);
}

public sealed record CapLenientPayBandBundleAdjuster(double Cap) : ILenientPayBandBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPayBandBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LenientPayBandBundleVersionOffsetAdjuster(double Weight) : ILenientPayBandBundleStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(LenientPayBandBundle item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumLenientPayBandBundleAdjuster(double Minimum) : ILenientPayBandBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPayBandBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient pay band bundle steps in order and keeps a trace of each.</summary>
public sealed class LenientPayBandBundleEngine(List<ILenientPayBandBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPayBandBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPayBandBundleEngine DefaultLenientPayBandBundleEngine()
    {
        return new LenientPayBandBundleEngine(
            new List<ILenientPayBandBundleStep>
            {
                new CapLenientPayBandBundleAdjuster(41289.0),
                new LenientPayBandBundleVersionOffsetAdjuster(1.49),
                new MinimumLenientPayBandBundleAdjuster(44.6),
            },
            new List<string>(),
            2.5,
            69678.9);
    }

    public double Evaluate(LenientPayBandBundle item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPayBandBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
