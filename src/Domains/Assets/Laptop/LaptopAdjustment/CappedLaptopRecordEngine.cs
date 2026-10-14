namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped laptop record calculation.</summary>
public interface ICappedLaptopRecordFactor
{
    string Describe();
    double Adjust(CappedLaptopRecord item, double valueValue);
}

public sealed record RoundingCappedLaptopRecordAdjuster(double Step) : ICappedLaptopRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedLaptopRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameCappedLaptopRecordAdjuster(Dictionary<string, double> Factors) : ICappedLaptopRecordFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedLaptopRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCappedLaptopRecordAdjuster(double Minimum) : ICappedLaptopRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedLaptopRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedLaptopRecordAdjuster(Registry Flags, string Flag, double Percent) : ICappedLaptopRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedLaptopRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped laptop record steps in order and keeps a trace of each.</summary>
public sealed class CappedLaptopRecordEngine(List<ICappedLaptopRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedLaptopRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedLaptopRecordEngine DefaultCappedLaptopRecordEngine(Registry flags)
    {
        return new CappedLaptopRecordEngine(
            new List<ICappedLaptopRecordFactor>
            {
                new RoundingCappedLaptopRecordAdjuster(0.1),
                new NameCappedLaptopRecordAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.18, ["south"] = 1.39 }),
                new MinimumCappedLaptopRecordAdjuster(15.0),
                new FlaggedCappedLaptopRecordAdjuster(flags, "capped-laptop-record-strict", 6.7),
            },
            new List<string>(),
            4.6,
            91289.9);
    }

    public double Evaluate(CappedLaptopRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedLaptopRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
