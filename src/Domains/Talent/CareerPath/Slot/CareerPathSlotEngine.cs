namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path slot calculation.</summary>
public interface ICareerPathSlotFactor
{
    string Describe();
    double Adjust(CareerPathSlot item, double valueValue);
}

public sealed record FlaggedCareerPathSlotAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathSlotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryCareerPathSlotAdjuster(Dictionary<string, double> Factors) : ICareerPathSlotFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(CareerPathSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CareerPathSlotBudgetOffsetAdjuster(double Weight) : ICareerPathSlotFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(CareerPathSlot item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapCareerPathSlotAdjuster(double Cap) : ICareerPathSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCareerPathSlotAdjuster(double Step) : ICareerPathSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the career path slot steps in order and keeps a trace of each.</summary>
public sealed class CareerPathSlotEngine(List<ICareerPathSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathSlotEngine DefaultCareerPathSlotEngine(Registry flags)
    {
        return new CareerPathSlotEngine(
            new List<ICareerPathSlotFactor>
            {
                new FlaggedCareerPathSlotAdjuster(flags, "career-path-slot-strict", 2.4),
                new CountryCareerPathSlotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.84, ["west"] = 1.34 }),
                new CareerPathSlotBudgetOffsetAdjuster(1.21),
                new CapCareerPathSlotAdjuster(45516.0),
                new RoundingCareerPathSlotAdjuster(5.0),
            },
            new List<string>(),
            0.8,
            94540.4);
    }

    public double Run(CareerPathSlot item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
