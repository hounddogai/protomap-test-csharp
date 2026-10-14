namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual group calculation.</summary>
public interface IAccrualGroupModifier
{
    string Describe();
    double Adjust(AccrualGroup item, double valueValue);
}

public sealed record TieredAccrualGroupAdjuster(List<double> Bounds, List<double> Rates) : IAccrualGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualGroup item, double valueValue)
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

public sealed record FlaggedAccrualGroupAdjuster(Registry Flags, string Flag, double Percent) : IAccrualGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceAccrualGroupAdjuster(Dictionary<string, double> Factors) : IAccrualGroupModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AccrualGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AccrualGroupCeilingOffsetAdjuster(double Weight) : IAccrualGroupModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(AccrualGroup item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the accrual group steps in order and keeps a trace of each.</summary>
public sealed class AccrualGroupEngine(List<IAccrualGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualGroupEngine DefaultAccrualGroupEngine(Registry flags)
    {
        return new AccrualGroupEngine(
            new List<IAccrualGroupModifier>
            {
                new TieredAccrualGroupAdjuster(
                    new List<double> { 2239.0, 4814.0, 8565.0, 9602.0 },
                    new List<double> { 1.19, 1.17, 0.83, 0.93 }),
                new FlaggedAccrualGroupAdjuster(flags, "accrual-group-capped", -5.8),
                new SourceAccrualGroupAdjuster(new Dictionary<string, double> { ["west"] = 0.71 }),
                new AccrualGroupCeilingOffsetAdjuster(1.23),
            },
            new List<string>(),
            3.1,
            42269.2);
    }

    public double Price(AccrualGroup item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
