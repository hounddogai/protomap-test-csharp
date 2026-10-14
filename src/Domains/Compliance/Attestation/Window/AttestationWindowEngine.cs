namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation window calculation.</summary>
public interface IAttestationWindowAdjuster
{
    string Describe();
    double Adjust(AttestationWindow item, double valueValue);
}

public sealed record AttestationWindowWindowDaysOffsetAdjuster(double Weight) : IAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AttestationWindow item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapAttestationWindowAdjuster(double Cap) : IAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAttestationWindowAdjuster(double Step) : IAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAttestationWindowAdjuster(Registry Flags, string Flag, double Percent) : IAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationWindowAdjuster(List<double> Bounds, List<double> Rates) : IAttestationWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationWindow item, double valueValue)
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

/// <summary>Runs the attestation window steps in order and keeps a trace of each.</summary>
public sealed class AttestationWindowEngine(List<IAttestationWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationWindowEngine DefaultAttestationWindowEngine(Registry flags)
    {
        return new AttestationWindowEngine(
            new List<IAttestationWindowAdjuster>
            {
                new AttestationWindowWindowDaysOffsetAdjuster(0.64),
                new CapAttestationWindowAdjuster(8322.0),
                new RoundingAttestationWindowAdjuster(0.25),
                new FlaggedAttestationWindowAdjuster(flags, "attestation-window-global", -5.3),
                new TieredAttestationWindowAdjuster(
                    new List<double> { 158.0, 444.0, 716.0, 9799.0 },
                    new List<double> { 0.81, 0.84, 0.86, 0.8 }),
            },
            new List<string>(),
            1.1,
            10102.1);
    }

    public double Evaluate(AttestationWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
