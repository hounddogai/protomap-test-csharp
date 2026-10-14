namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped attestation quota calculation.</summary>
public interface ICappedAttestationQuotaStep
{
    string Describe();
    double Adjust(CappedAttestationQuota item, double valueValue);
}

public sealed record MinimumCappedAttestationQuotaAdjuster(double Minimum) : ICappedAttestationQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAttestationQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentCappedAttestationQuotaAdjuster(Dictionary<string, double> Factors) : ICappedAttestationQuotaStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CappedAttestationQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CappedAttestationQuotaThresholdOffsetAdjuster(double Weight) : ICappedAttestationQuotaStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CappedAttestationQuota item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingCappedAttestationQuotaAdjuster(double Step) : ICappedAttestationQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAttestationQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedAttestationQuotaAdjuster(Registry Flags, string Flag, double Percent) : ICappedAttestationQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAttestationQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped attestation quota steps in order and keeps a trace of each.</summary>
public sealed class CappedAttestationQuotaEngine(List<ICappedAttestationQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAttestationQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAttestationQuotaEngine DefaultCappedAttestationQuotaEngine(Registry flags)
    {
        return new CappedAttestationQuotaEngine(
            new List<ICappedAttestationQuotaStep>
            {
                new MinimumCappedAttestationQuotaAdjuster(36.6),
                new SegmentCappedAttestationQuotaAdjuster(new Dictionary<string, double> { ["central"] = 1.16 }),
                new CappedAttestationQuotaThresholdOffsetAdjuster(1.98),
                new RoundingCappedAttestationQuotaAdjuster(1.0),
                new FlaggedCappedAttestationQuotaAdjuster(flags, "capped-attestation-quota-fallback", 4.7),
            },
            new List<string>(),
            4.1,
            21530.8);
    }

    public double Price(CappedAttestationQuota item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAttestationQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
