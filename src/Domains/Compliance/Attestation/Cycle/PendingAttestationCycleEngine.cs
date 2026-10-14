namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending attestation cycle calculation.</summary>
public interface IPendingAttestationCycleFactor
{
    string Describe();
    double Adjust(PendingAttestationCycle item, double valueValue);
}

public sealed record NamePendingAttestationCycleAdjuster(Dictionary<string, double> Factors) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingAttestationCycleAdjuster(double Minimum) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingAttestationCycleAdjuster(double Percent) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingAttestationCycleAdjuster(Registry Flags, string Flag, double Percent) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingAttestationCycleTenureMonthsOffsetAdjuster(double Weight) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record CapPendingAttestationCycleAdjuster(double Cap) : IPendingAttestationCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingAttestationCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pending attestation cycle steps in order and keeps a trace of each.</summary>
public sealed class PendingAttestationCycleEngine(List<IPendingAttestationCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAttestationCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAttestationCycleEngine DefaultPendingAttestationCycleEngine(Registry flags)
    {
        return new PendingAttestationCycleEngine(
            new List<IPendingAttestationCycleFactor>
            {
                new NamePendingAttestationCycleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.85, ["retail"] = 0.7 }),
                new MinimumPendingAttestationCycleAdjuster(20.5),
                new PercentPendingAttestationCycleAdjuster(3.0),
                new FlaggedPendingAttestationCycleAdjuster(flags, "pending-attestation-cycle-scheduled", 4.3),
                new PendingAttestationCycleTenureMonthsOffsetAdjuster(1.79),
                new CapPendingAttestationCycleAdjuster(15774.0),
            },
            new List<string>(),
            4.7,
            50037.0);
    }

    public double Compute(PendingAttestationCycle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingAttestationCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
