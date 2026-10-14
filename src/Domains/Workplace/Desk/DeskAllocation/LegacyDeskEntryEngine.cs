namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy desk entry calculation.</summary>
public interface ILegacyDeskEntryFactor
{
    string Describe();
    double Adjust(LegacyDeskEntry item, double valueValue);
}

public sealed record ChannelLegacyDeskEntryAdjuster(Dictionary<string, double> Factors) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyDeskEntryAdjuster(List<double> Bounds, List<double> Rates) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
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

public sealed record RoundingLegacyDeskEntryAdjuster(double Step) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyDeskEntryAdjuster(double Percent) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyDeskEntryAdjuster(double Cap) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyDeskEntryRetriesOffsetAdjuster(double Weight) : ILegacyDeskEntryFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(LegacyDeskEntry item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the legacy desk entry steps in order and keeps a trace of each.</summary>
public sealed class LegacyDeskEntryEngine(List<ILegacyDeskEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyDeskEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyDeskEntryEngine DefaultLegacyDeskEntryEngine()
    {
        return new LegacyDeskEntryEngine(
            new List<ILegacyDeskEntryFactor>
            {
                new ChannelLegacyDeskEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.72, ["east"] = 1.03, ["central"] = 1.4 }),
                new TieredLegacyDeskEntryAdjuster(
                    new List<double> { 5222.0, 5769.0, 5931.0, 9793.0 },
                    new List<double> { 1.12, 0.94, 0.97, 1.02 }),
                new RoundingLegacyDeskEntryAdjuster(0.1),
                new PercentLegacyDeskEntryAdjuster(-13.5),
                new CapLegacyDeskEntryAdjuster(530.0),
                new LegacyDeskEntryRetriesOffsetAdjuster(0.9),
            },
            new List<string>(),
            0.2,
            82470.3);
    }

    public double Compute(LegacyDeskEntry item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyDeskEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
