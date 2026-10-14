namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat record calculation.</summary>
public interface ILicenseSeatRecordModifier
{
    string Describe();
    double Adjust(LicenseSeatRecord item, double valueValue);
}

public sealed record CapLicenseSeatRecordAdjuster(double Cap) : ILicenseSeatRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LicenseSeatRecordWindowDaysOffsetAdjuster(double Weight) : ILicenseSeatRecordModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(LicenseSeatRecord item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedLicenseSeatRecordAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat record steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatRecordEngine(List<ILicenseSeatRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatRecordEngine DefaultLicenseSeatRecordEngine(Registry flags)
    {
        return new LicenseSeatRecordEngine(
            new List<ILicenseSeatRecordModifier>
            {
                new CapLicenseSeatRecordAdjuster(20624.0),
                new LicenseSeatRecordWindowDaysOffsetAdjuster(0.09),
                new FlaggedLicenseSeatRecordAdjuster(flags, "license-seat-record-pending", 4.6),
            },
            new List<string>(),
            4.5,
            96745.3);
    }

    public double Price(LicenseSeatRecord item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
