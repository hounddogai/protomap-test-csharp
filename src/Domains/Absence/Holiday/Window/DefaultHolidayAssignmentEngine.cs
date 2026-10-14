namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default holiday assignment calculation.</summary>
public interface IDefaultHolidayAssignmentModifier
{
    string Describe();
    double Adjust(DefaultHolidayAssignment item, double valueValue);
}

public sealed record TieredDefaultHolidayAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IDefaultHolidayAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultHolidayAssignment item, double valueValue)
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

public sealed record UnitDefaultHolidayAssignmentAdjuster(Dictionary<string, double> Factors) : IDefaultHolidayAssignmentModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DefaultHolidayAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultHolidayAssignmentAdjuster(double Percent) : IDefaultHolidayAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultHolidayAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultHolidayAssignmentAdjuster(double Step) : IDefaultHolidayAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHolidayAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultHolidayAssignmentBudgetOffsetAdjuster(double Weight) : IDefaultHolidayAssignmentModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(DefaultHolidayAssignment item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the default holiday assignment steps in order and keeps a trace of each.</summary>
public sealed class DefaultHolidayAssignmentEngine(List<IDefaultHolidayAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHolidayAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultHolidayAssignmentEngine DefaultDefaultHolidayAssignmentEngine()
    {
        return new DefaultHolidayAssignmentEngine(
            new List<IDefaultHolidayAssignmentModifier>
            {
                new TieredDefaultHolidayAssignmentAdjuster(
                    new List<double> { 865.0, 6344.0 },
                    new List<double> { 0.86, 0.87 }),
                new UnitDefaultHolidayAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.06, ["retail"] = 0.91, ["north"] = 0.93 }),
                new PercentDefaultHolidayAssignmentAdjuster(15.5),
                new RoundingDefaultHolidayAssignmentAdjuster(5.0),
                new DefaultHolidayAssignmentBudgetOffsetAdjuster(1.64),
            },
            new List<string>(),
            2.4,
            32480.5);
    }

    public double Price(DefaultHolidayAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHolidayAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
