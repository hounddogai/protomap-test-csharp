namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount group calculation.</summary>
public interface IHeadcountGroupFactor
{
    string Describe();
    double Adjust(HeadcountGroup item, double valueValue);
}

public sealed record HeadcountGroupFloorValueOffsetAdjuster(double Weight) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentHeadcountGroupAdjuster(double Percent) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameHeadcountGroupAdjuster(Dictionary<string, double> Factors) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredHeadcountGroupAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
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

public sealed record RoundingHeadcountGroupAdjuster(double Step) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapHeadcountGroupAdjuster(double Cap) : IHeadcountGroupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the headcount group steps in order and keeps a trace of each.</summary>
public sealed class HeadcountGroupEngine(List<IHeadcountGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountGroupEngine DefaultHeadcountGroupEngine()
    {
        return new HeadcountGroupEngine(
            new List<IHeadcountGroupFactor>
            {
                new HeadcountGroupFloorValueOffsetAdjuster(0.7),
                new PercentHeadcountGroupAdjuster(-23.0),
                new NameHeadcountGroupAdjuster(new Dictionary<string, double> { ["east"] = 1.19, ["north"] = 0.83 }),
                new TieredHeadcountGroupAdjuster(
                    new List<double> { 5507.0, 6419.0, 9291.0 },
                    new List<double> { 0.81, 1.11, 1.1 }),
                new RoundingHeadcountGroupAdjuster(5.0),
                new CapHeadcountGroupAdjuster(47526.0),
            },
            new List<string>(),
            1.0,
            92081.6);
    }

    public double Evaluate(HeadcountGroup item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
