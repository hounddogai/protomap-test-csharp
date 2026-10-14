namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback span ledger calculation.</summary>
public interface IFallbackSpanLedgerAdjuster
{
    string Describe();
    double Adjust(FallbackSpanLedger item, double valueValue);
}

public sealed record CapFallbackSpanLedgerAdjuster(double Cap) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackSpanLedgerAdjuster(List<double> Bounds, List<double> Rates) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
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

public sealed record FallbackSpanLedgerWindowDaysOffsetAdjuster(double Weight) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record NameFallbackSpanLedgerAdjuster(Dictionary<string, double> Factors) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackSpanLedgerAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackSpanLedgerAdjuster(double Minimum) : IFallbackSpanLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackSpanLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback span ledger steps in order and keeps a trace of each.</summary>
public sealed class FallbackSpanLedgerEngine(List<IFallbackSpanLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSpanLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSpanLedgerEngine DefaultFallbackSpanLedgerEngine(Registry flags)
    {
        return new FallbackSpanLedgerEngine(
            new List<IFallbackSpanLedgerAdjuster>
            {
                new CapFallbackSpanLedgerAdjuster(2413.0),
                new TieredFallbackSpanLedgerAdjuster(
                    new List<double> { 3388.0, 6740.0, 7827.0, 8489.0 },
                    new List<double> { 0.86, 1.2, 1.04, 0.88 }),
                new FallbackSpanLedgerWindowDaysOffsetAdjuster(0.92),
                new NameFallbackSpanLedgerAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.04, ["east"] = 0.78, ["retail"] = 1.2 }),
                new FlaggedFallbackSpanLedgerAdjuster(flags, "fallback-span-ledger-default", 3.3),
                new MinimumFallbackSpanLedgerAdjuster(0.1),
            },
            new List<string>(),
            0.4,
            95338.1);
    }

    public double Evaluate(FallbackSpanLedger item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSpanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
