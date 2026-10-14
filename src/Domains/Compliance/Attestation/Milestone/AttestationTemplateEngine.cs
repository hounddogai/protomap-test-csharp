namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation template calculation.</summary>
public interface IAttestationTemplateStep
{
    string Describe();
    double Adjust(AttestationTemplate item, double valueValue);
}

public sealed record FlaggedAttestationTemplateAdjuster(Registry Flags, string Flag, double Percent) : IAttestationTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationTemplateAdjuster(List<double> Bounds, List<double> Rates) : IAttestationTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationTemplate item, double valueValue)
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

public sealed record RoundingAttestationTemplateAdjuster(double Step) : IAttestationTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation template steps in order and keeps a trace of each.</summary>
public sealed class AttestationTemplateEngine(List<IAttestationTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationTemplateEngine DefaultAttestationTemplateEngine(Registry flags)
    {
        return new AttestationTemplateEngine(
            new List<IAttestationTemplateStep>
            {
                new FlaggedAttestationTemplateAdjuster(flags, "attestation-template-strict", 8.4),
                new TieredAttestationTemplateAdjuster(
                    new List<double> { 169.0, 1585.0, 4586.0, 9499.0 },
                    new List<double> { 1.2, 1.16, 0.85, 1.06 }),
                new RoundingAttestationTemplateAdjuster(0.05),
            },
            new List<string>(),
            1.4,
            23338.4);
    }

    public double Run(AttestationTemplate item)
    {
        var current = item.EffectiveLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AttestationTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
