namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict attestation estimate calculation.</summary>
public interface IStrictAttestationEstimateAdjuster
{
    string Describe();
    double Adjust(StrictAttestationEstimate item, double valueValue);
}

public sealed record CapStrictAttestationEstimateAdjuster(double Cap) : IStrictAttestationEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAttestationEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictAttestationEstimateAdjuster(double Step) : IStrictAttestationEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAttestationEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictAttestationEstimateAdjuster(double Minimum) : IStrictAttestationEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAttestationEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameStrictAttestationEstimateAdjuster(Dictionary<string, double> Factors) : IStrictAttestationEstimateAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictAttestationEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict attestation estimate steps in order and keeps a trace of each.</summary>
public sealed class StrictAttestationEstimateEngine(List<IStrictAttestationEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAttestationEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAttestationEstimateEngine DefaultStrictAttestationEstimateEngine()
    {
        return new StrictAttestationEstimateEngine(
            new List<IStrictAttestationEstimateAdjuster>
            {
                new CapStrictAttestationEstimateAdjuster(43102.0),
                new RoundingStrictAttestationEstimateAdjuster(0.5),
                new MinimumStrictAttestationEstimateAdjuster(4.9),
                new NameStrictAttestationEstimateAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.36, ["south"] = 1.06, ["online"] = 0.91 }),
            },
            new List<string>(),
            2.7,
            96088.3);
    }

    public double Run(StrictAttestationEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictAttestationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
