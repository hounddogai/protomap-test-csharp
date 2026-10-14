namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal leave request budget calculation.</summary>
public interface ISeasonalLeaveRequestBudgetModifier
{
    string Describe();
    double Adjust(SeasonalLeaveRequestBudget item, double valueValue);
}

public sealed record MinimumSeasonalLeaveRequestBudgetAdjuster(double Minimum) : ISeasonalLeaveRequestBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLeaveRequestBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSeasonalLeaveRequestBudgetAdjuster(double Step) : ISeasonalLeaveRequestBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalLeaveRequestBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSeasonalLeaveRequestBudgetAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalLeaveRequestBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalLeaveRequestBudget item, double valueValue)
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

public sealed record SeasonalLeaveRequestBudgetFloorValueOffsetAdjuster(double Weight) : ISeasonalLeaveRequestBudgetModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SeasonalLeaveRequestBudget item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the seasonal leave request budget steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLeaveRequestBudgetEngine(List<ISeasonalLeaveRequestBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLeaveRequestBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLeaveRequestBudgetEngine DefaultSeasonalLeaveRequestBudgetEngine()
    {
        return new SeasonalLeaveRequestBudgetEngine(
            new List<ISeasonalLeaveRequestBudgetModifier>
            {
                new MinimumSeasonalLeaveRequestBudgetAdjuster(27.4),
                new RoundingSeasonalLeaveRequestBudgetAdjuster(0.1),
                new TieredSeasonalLeaveRequestBudgetAdjuster(
                    new List<double> { 2653.0, 9205.0 },
                    new List<double> { 0.87, 0.9 }),
                new SeasonalLeaveRequestBudgetFloorValueOffsetAdjuster(1.29),
            },
            new List<string>(),
            0.8,
            59121.4);
    }

    public double Compute(SeasonalLeaveRequestBudget item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLeaveRequestBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
