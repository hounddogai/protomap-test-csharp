namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered mobility audit calculation.</summary>
public interface ITieredMobilityAuditModifier
{
    string Describe();
    double Adjust(TieredMobilityAudit item, double valueValue);
}

public sealed record CapTieredMobilityAuditAdjuster(double Cap) : ITieredMobilityAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredMobilityAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTieredMobilityAuditAdjuster(List<double> Bounds, List<double> Rates) : ITieredMobilityAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredMobilityAudit item, double valueValue)
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

public sealed record NameTieredMobilityAuditAdjuster(Dictionary<string, double> Factors) : ITieredMobilityAuditModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredMobilityAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTieredMobilityAuditAdjuster(double Step) : ITieredMobilityAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredMobilityAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTieredMobilityAuditAdjuster(double Percent) : ITieredMobilityAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredMobilityAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered mobility audit steps in order and keeps a trace of each.</summary>
public sealed class TieredMobilityAuditEngine(List<ITieredMobilityAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredMobilityAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredMobilityAuditEngine DefaultTieredMobilityAuditEngine()
    {
        return new TieredMobilityAuditEngine(
            new List<ITieredMobilityAuditModifier>
            {
                new CapTieredMobilityAuditAdjuster(36277.0),
                new TieredTieredMobilityAuditAdjuster(
                    new List<double> { 219.0, 1480.0, 2606.0, 5382.0 },
                    new List<double> { 0.93, 0.85, 1.17, 1.14 }),
                new NameTieredMobilityAuditAdjuster(new Dictionary<string, double> { ["east"] = 1.04 }),
                new RoundingTieredMobilityAuditAdjuster(0.25),
                new PercentTieredMobilityAuditAdjuster(10.5),
            },
            new List<string>(),
            3.0,
            40509.8);
    }

    public double Evaluate(TieredMobilityAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredMobilityAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
