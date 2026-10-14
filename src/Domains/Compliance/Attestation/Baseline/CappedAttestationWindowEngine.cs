namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped attestation window calculation.</summary>
public interface ICappedAttestationWindowAdjuster
{
    string Describe();
    double Adjust(CappedAttestationWindow item, double valueValue);
}

public sealed record LocaleCappedAttestationWindowAdjuster(Dictionary<string, double> Factors) : ICappedAttestationWindowAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CappedAttestationWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedAttestationWindowAdjuster(double Cap) : ICappedAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAttestationWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedAttestationWindowAdjuster(Registry Flags, string Flag, double Percent) : ICappedAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAttestationWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped attestation window steps in order and keeps a trace of each.</summary>
public sealed class CappedAttestationWindowEngine(List<ICappedAttestationWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAttestationWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAttestationWindowEngine DefaultCappedAttestationWindowEngine(Registry flags)
    {
        return new CappedAttestationWindowEngine(
            new List<ICappedAttestationWindowAdjuster>
            {
                new LocaleCappedAttestationWindowAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.23, ["east"] = 1.13, ["north"] = 1.21 }),
                new CapCappedAttestationWindowAdjuster(38483.0),
                new FlaggedCappedAttestationWindowAdjuster(flags, "capped-attestation-window-adaptive", -4.3),
            },
            new List<string>(),
            0.8,
            94385.1);
    }

    public double Run(CappedAttestationWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedAttestationWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
