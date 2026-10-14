namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive career path slot calculation.</summary>
public interface IAdaptiveCareerPathSlotFactor
{
    string Describe();
    double Adjust(AdaptiveCareerPathSlot item, double valueValue);
}

public sealed record RoundingAdaptiveCareerPathSlotAdjuster(double Step) : IAdaptiveCareerPathSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveCareerPathSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveCareerPathSlotAdjuster(double Minimum) : IAdaptiveCareerPathSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCareerPathSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveCareerPathSlotAdjuster(double Cap) : IAdaptiveCareerPathSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCareerPathSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveCareerPathSlotVersionOffsetAdjuster(double Weight) : IAdaptiveCareerPathSlotFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AdaptiveCareerPathSlot item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the adaptive career path slot steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCareerPathSlotEngine(List<IAdaptiveCareerPathSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCareerPathSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCareerPathSlotEngine DefaultAdaptiveCareerPathSlotEngine()
    {
        return new AdaptiveCareerPathSlotEngine(
            new List<IAdaptiveCareerPathSlotFactor>
            {
                new RoundingAdaptiveCareerPathSlotAdjuster(0.25),
                new MinimumAdaptiveCareerPathSlotAdjuster(0.2),
                new CapAdaptiveCareerPathSlotAdjuster(4967.0),
                new AdaptiveCareerPathSlotVersionOffsetAdjuster(0.86),
            },
            new List<string>(),
            3.3,
            39661.4);
    }

    public double Run(AdaptiveCareerPathSlot item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCareerPathSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
