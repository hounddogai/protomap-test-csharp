namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat ledger calculation.</summary>
public interface ILicenseSeatLedgerAdjuster
{
    string Describe();
    double Adjust(LicenseSeatLedger item, double valueValue);
}

public sealed record LabelLicenseSeatLedgerAdjuster(Dictionary<string, double> Factors) : ILicenseSeatLedgerAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LicenseSeatLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLicenseSeatLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLicenseSeatLedgerAdjuster(double Minimum) : ILicenseSeatLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LicenseSeatLedgerMarginOffsetAdjuster(double Weight) : ILicenseSeatLedgerAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(LicenseSeatLedger item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingLicenseSeatLedgerAdjuster(double Step) : ILicenseSeatLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the license seat ledger steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatLedgerEngine(List<ILicenseSeatLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatLedgerEngine DefaultLicenseSeatLedgerEngine(Registry flags)
    {
        return new LicenseSeatLedgerEngine(
            new List<ILicenseSeatLedgerAdjuster>
            {
                new LabelLicenseSeatLedgerAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.27, ["online"] = 1.37 }),
                new FlaggedLicenseSeatLedgerAdjuster(flags, "license-seat-ledger-global", -3.1),
                new MinimumLicenseSeatLedgerAdjuster(10.1),
                new LicenseSeatLedgerMarginOffsetAdjuster(1.2),
                new RoundingLicenseSeatLedgerAdjuster(0.1),
            },
            new List<string>(),
            3.6,
            51413.5);
    }

    public double Price(LicenseSeatLedger item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
