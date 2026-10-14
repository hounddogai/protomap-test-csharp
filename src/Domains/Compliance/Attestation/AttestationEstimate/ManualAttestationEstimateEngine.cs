namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual attestation estimate calculation.</summary>
public interface IManualAttestationEstimateStep
{
    string Describe();
    double Adjust(ManualAttestationEstimate item, double valueValue);
}

public sealed record FlaggedManualAttestationEstimateAdjuster(Registry Flags, string Flag, double Percent) : IManualAttestationEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAttestationEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualAttestationEstimateAdjuster(double Step) : IManualAttestationEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAttestationEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentManualAttestationEstimateAdjuster(double Percent) : IManualAttestationEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualAttestationEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualAttestationEstimateAdjuster(double Cap) : IManualAttestationEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAttestationEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual attestation estimate steps in order and keeps a trace of each.</summary>
public sealed class ManualAttestationEstimateEngine(List<IManualAttestationEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAttestationEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAttestationEstimateEngine DefaultManualAttestationEstimateEngine(Registry flags)
    {
        return new ManualAttestationEstimateEngine(
            new List<IManualAttestationEstimateStep>
            {
                new FlaggedManualAttestationEstimateAdjuster(flags, "manual-attestation-estimate-strict", 1.8),
                new RoundingManualAttestationEstimateAdjuster(5.0),
                new PercentManualAttestationEstimateAdjuster(5.7),
                new CapManualAttestationEstimateAdjuster(49943.0),
            },
            new List<string>(),
            2.2,
            55984.9);
    }

    public double Compute(ManualAttestationEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualAttestationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
