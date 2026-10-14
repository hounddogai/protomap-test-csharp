namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped desk record calculation.</summary>
public interface ICappedDeskRecordFactor
{
    string Describe();
    double Adjust(CappedDeskRecord item, double valueValue);
}

public sealed record FlaggedCappedDeskRecordAdjuster(Registry Flags, string Flag, double Percent) : ICappedDeskRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedDeskRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedDeskRecordExpiresAtOffsetAdjuster(double Weight) : ICappedDeskRecordFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(CappedDeskRecord item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record CapCappedDeskRecordAdjuster(double Cap) : ICappedDeskRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedDeskRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedDeskRecordAdjuster(double Step) : ICappedDeskRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedDeskRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped desk record steps in order and keeps a trace of each.</summary>
public sealed class CappedDeskRecordEngine(List<ICappedDeskRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedDeskRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedDeskRecordEngine DefaultCappedDeskRecordEngine(Registry flags)
    {
        return new CappedDeskRecordEngine(
            new List<ICappedDeskRecordFactor>
            {
                new FlaggedCappedDeskRecordAdjuster(flags, "capped-desk-record-default", -8.7),
                new CappedDeskRecordExpiresAtOffsetAdjuster(1.64),
                new CapCappedDeskRecordAdjuster(25307.0),
                new RoundingCappedDeskRecordAdjuster(5.0),
            },
            new List<string>(),
            2.5,
            11703.9);
    }

    public double Evaluate(CappedDeskRecord item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedDeskRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
