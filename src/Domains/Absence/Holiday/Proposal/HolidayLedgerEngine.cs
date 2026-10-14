namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday ledger calculation.</summary>
public interface IHolidayLedgerModifier
{
    string Describe();
    double Adjust(HolidayLedger item, double valueValue);
}

public sealed record RoundingHolidayLedgerAdjuster(double Step) : IHolidayLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHolidayLedgerAdjuster(double Minimum) : IHolidayLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelHolidayLedgerAdjuster(Dictionary<string, double> Factors) : IHolidayLedgerModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(HolidayLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedHolidayLedgerAdjuster(Registry Flags, string Flag, double Percent) : IHolidayLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHolidayLedgerAdjuster(double Percent) : IHolidayLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the holiday ledger steps in order and keeps a trace of each.</summary>
public sealed class HolidayLedgerEngine(List<IHolidayLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayLedgerEngine DefaultHolidayLedgerEngine(Registry flags)
    {
        return new HolidayLedgerEngine(
            new List<IHolidayLedgerModifier>
            {
                new RoundingHolidayLedgerAdjuster(0.25),
                new MinimumHolidayLedgerAdjuster(24.4),
                new LabelHolidayLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.39, ["east"] = 0.94, ["west"] = 0.95 }),
                new FlaggedHolidayLedgerAdjuster(flags, "holiday-ledger-pending", -8.6),
                new PercentHolidayLedgerAdjuster(-2.0),
            },
            new List<string>(),
            4.1,
            36779.6);
    }

    public double Price(HolidayLedger item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
