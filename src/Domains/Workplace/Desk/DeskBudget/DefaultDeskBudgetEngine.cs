namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default desk budget calculation.</summary>
public interface IDefaultDeskBudgetModifier
{
    string Describe();
    double Adjust(DefaultDeskBudget item, double valueValue);
}

public sealed record TieredDefaultDeskBudgetAdjuster(List<double> Bounds, List<double> Rates) : IDefaultDeskBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultDeskBudget item, double valueValue)
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

public sealed record PercentDefaultDeskBudgetAdjuster(double Percent) : IDefaultDeskBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultDeskBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultDeskBudgetBudgetOffsetAdjuster(double Weight) : IDefaultDeskBudgetModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(DefaultDeskBudget item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingDefaultDeskBudgetAdjuster(double Step) : IDefaultDeskBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultDeskBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultDeskBudgetAdjuster(double Minimum) : IDefaultDeskBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultDeskBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default desk budget steps in order and keeps a trace of each.</summary>
public sealed class DefaultDeskBudgetEngine(List<IDefaultDeskBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultDeskBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultDeskBudgetEngine DefaultDefaultDeskBudgetEngine()
    {
        return new DefaultDeskBudgetEngine(
            new List<IDefaultDeskBudgetModifier>
            {
                new TieredDefaultDeskBudgetAdjuster(
                    new List<double> { 5559.0, 6333.0 },
                    new List<double> { 1.02, 1.12 }),
                new PercentDefaultDeskBudgetAdjuster(-3.8),
                new DefaultDeskBudgetBudgetOffsetAdjuster(1.92),
                new RoundingDefaultDeskBudgetAdjuster(0.1),
                new MinimumDefaultDeskBudgetAdjuster(49.7),
            },
            new List<string>(),
            0.3,
            3723.5);
    }

    public double Run(DefaultDeskBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultDeskBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
