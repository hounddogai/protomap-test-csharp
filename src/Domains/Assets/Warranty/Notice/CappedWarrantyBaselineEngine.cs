namespace Acme.Workforce.Assets.Warranty.Notice;

/// <summary>One step of the capped warranty baseline calculation.</summary>
public interface ICappedWarrantyBaselineStep
{
    string Describe();
    double Adjust(CappedWarrantyBaseline item, double valueValue);
}

public sealed record CategoryCappedWarrantyBaselineAdjuster(Dictionary<string, double> Factors) : ICappedWarrantyBaselineStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CappedWarrantyBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCappedWarrantyBaselineAdjuster(double Minimum) : ICappedWarrantyBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedWarrantyBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedWarrantyBaselineAdjuster(double Percent) : ICappedWarrantyBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedWarrantyBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped warranty baseline steps in order and keeps a trace of each.</summary>
public sealed class CappedWarrantyBaselineEngine(List<ICappedWarrantyBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedWarrantyBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedWarrantyBaselineEngine DefaultCappedWarrantyBaselineEngine()
    {
        return new CappedWarrantyBaselineEngine(
            new List<ICappedWarrantyBaselineStep>
            {
                new CategoryCappedWarrantyBaselineAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.31, ["south"] = 1.05, ["north"] = 1.28 }),
                new MinimumCappedWarrantyBaselineAdjuster(40.5),
                new PercentCappedWarrantyBaselineAdjuster(1.3),
            },
            new List<string>(),
            1.7,
            12596.9);
    }

    public double Price(CappedWarrantyBaseline item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedWarrantyBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
