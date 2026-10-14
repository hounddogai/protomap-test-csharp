namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default attestation template calculation.</summary>
public interface IDefaultAttestationTemplateStep
{
    string Describe();
    double Adjust(DefaultAttestationTemplate item, double valueValue);
}

public sealed record FlaggedDefaultAttestationTemplateAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultAttestationTemplateAdjuster(double Step) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDefaultAttestationTemplateAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
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

public sealed record ChannelDefaultAttestationTemplateAdjuster(Dictionary<string, double> Factors) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultAttestationTemplateAdjuster(double Cap) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultAttestationTemplateAdjuster(double Minimum) : IDefaultAttestationTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAttestationTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default attestation template steps in order and keeps a trace of each.</summary>
public sealed class DefaultAttestationTemplateEngine(List<IDefaultAttestationTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAttestationTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAttestationTemplateEngine DefaultDefaultAttestationTemplateEngine(Registry flags)
    {
        return new DefaultAttestationTemplateEngine(
            new List<IDefaultAttestationTemplateStep>
            {
                new FlaggedDefaultAttestationTemplateAdjuster(flags, "default-attestation-template-seasonal", -3.6),
                new RoundingDefaultAttestationTemplateAdjuster(0.05),
                new TieredDefaultAttestationTemplateAdjuster(
                    new List<double> { 708.0, 1583.0, 6796.0 },
                    new List<double> { 0.94, 0.98, 0.94 }),
                new ChannelDefaultAttestationTemplateAdjuster(new Dictionary<string, double> { ["south"] = 0.98 }),
                new CapDefaultAttestationTemplateAdjuster(16320.0),
                new MinimumDefaultAttestationTemplateAdjuster(42.2),
            },
            new List<string>(),
            2.9,
            34144.1);
    }

    public double Price(DefaultAttestationTemplate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAttestationTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
