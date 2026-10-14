namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation notice calculation.</summary>
public interface IAttestationNoticeModifier
{
    string Describe();
    double Adjust(AttestationNotice item, double valueValue);
}

public sealed record MinimumAttestationNoticeAdjuster(double Minimum) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleAttestationNoticeAdjuster(Dictionary<string, double> Factors) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AttestationNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAttestationNoticeAdjuster(double Cap) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AttestationNoticeRevisionOffsetAdjuster(double Weight) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AttestationNotice item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record TieredAttestationNoticeAdjuster(List<double> Bounds, List<double> Rates) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationNotice item, double valueValue)
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

public sealed record FlaggedAttestationNoticeAdjuster(Registry Flags, string Flag, double Percent) : IAttestationNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation notice steps in order and keeps a trace of each.</summary>
public sealed class AttestationNoticeEngine(List<IAttestationNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationNoticeEngine DefaultAttestationNoticeEngine(Registry flags)
    {
        return new AttestationNoticeEngine(
            new List<IAttestationNoticeModifier>
            {
                new MinimumAttestationNoticeAdjuster(2.7),
                new TitleAttestationNoticeAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.92, ["west"] = 1.12 }),
                new CapAttestationNoticeAdjuster(31477.0),
                new AttestationNoticeRevisionOffsetAdjuster(1.7),
                new TieredAttestationNoticeAdjuster(
                    new List<double> { 764.0, 6944.0, 7797.0 },
                    new List<double> { 1.09, 0.88, 1.01 }),
                new FlaggedAttestationNoticeAdjuster(flags, "attestation-notice-seasonal", -8.4),
            },
            new List<string>(),
            0.4,
            57549.3);
    }

    public double Evaluate(AttestationNotice item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
