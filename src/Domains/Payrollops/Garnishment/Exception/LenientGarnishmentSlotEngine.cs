namespace Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>One step of the lenient garnishment slot calculation.</summary>
public interface ILenientGarnishmentSlotModifier
{
    string Describe();
    double Adjust(LenientGarnishmentSlot item, double valueValue);
}

public sealed record PercentLenientGarnishmentSlotAdjuster(double Percent) : ILenientGarnishmentSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientGarnishmentSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientGarnishmentSlotAdjuster(double Minimum) : ILenientGarnishmentSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientGarnishmentSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientGarnishmentSlotDiscountOffsetAdjuster(double Weight) : ILenientGarnishmentSlotModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LenientGarnishmentSlot item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the lenient garnishment slot steps in order and keeps a trace of each.</summary>
public sealed class LenientGarnishmentSlotEngine(List<ILenientGarnishmentSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientGarnishmentSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientGarnishmentSlotEngine DefaultLenientGarnishmentSlotEngine()
    {
        return new LenientGarnishmentSlotEngine(
            new List<ILenientGarnishmentSlotModifier>
            {
                new PercentLenientGarnishmentSlotAdjuster(-12.1),
                new MinimumLenientGarnishmentSlotAdjuster(12.2),
                new LenientGarnishmentSlotDiscountOffsetAdjuster(1.81),
            },
            new List<string>(),
            3.4,
            12595.3);
    }

    public double Compute(LenientGarnishmentSlot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientGarnishmentSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
