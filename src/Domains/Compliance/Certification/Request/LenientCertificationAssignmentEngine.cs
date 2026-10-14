namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient certification assignment calculation.</summary>
public interface ILenientCertificationAssignmentModifier
{
    string Describe();
    double Adjust(LenientCertificationAssignment item, double valueValue);
}

public sealed record RoundingLenientCertificationAssignmentAdjuster(double Step) : ILenientCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientCertificationAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientCertificationAssignmentAdjuster(double Cap) : ILenientCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientCertificationAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleLenientCertificationAssignmentAdjuster(Dictionary<string, double> Factors) : ILenientCertificationAssignmentModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LenientCertificationAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientCertificationAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientCertificationAssignment item, double valueValue)
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

/// <summary>Runs the lenient certification assignment steps in order and keeps a trace of each.</summary>
public sealed class LenientCertificationAssignmentEngine(List<ILenientCertificationAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientCertificationAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientCertificationAssignmentEngine DefaultLenientCertificationAssignmentEngine()
    {
        return new LenientCertificationAssignmentEngine(
            new List<ILenientCertificationAssignmentModifier>
            {
                new RoundingLenientCertificationAssignmentAdjuster(0.5),
                new CapLenientCertificationAssignmentAdjuster(2851.0),
                new LocaleLenientCertificationAssignmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.86, ["west"] = 1.36 }),
                new TieredLenientCertificationAssignmentAdjuster(
                    new List<double> { 2000.0, 4531.0, 8804.0, 9133.0 },
                    new List<double> { 0.88, 1.0, 0.93, 0.92 }),
            },
            new List<string>(),
            2.2,
            44716.7);
    }

    public double Price(LenientCertificationAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientCertificationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
