namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall template calculation.</summary>
public interface ITownHallTemplateAdjuster
{
    string Describe();
    double Adjust(TownHallTemplate item, double valueValue);
}

public sealed record TieredTownHallTemplateAdjuster(List<double> Bounds, List<double> Rates) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
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

public sealed record TownHallTemplateEndsAtOffsetAdjuster(double Weight) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapTownHallTemplateAdjuster(double Cap) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTownHallTemplateAdjuster(double Minimum) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTownHallTemplateAdjuster(double Step) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTownHallTemplateAdjuster(double Percent) : ITownHallTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall template steps in order and keeps a trace of each.</summary>
public sealed class TownHallTemplateEngine(List<ITownHallTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallTemplateEngine DefaultTownHallTemplateEngine()
    {
        return new TownHallTemplateEngine(
            new List<ITownHallTemplateAdjuster>
            {
                new TieredTownHallTemplateAdjuster(
                    new List<double> { 142.0, 9735.0, 9946.0 },
                    new List<double> { 0.82, 1.08, 0.81 }),
                new TownHallTemplateEndsAtOffsetAdjuster(0.94),
                new CapTownHallTemplateAdjuster(36025.0),
                new MinimumTownHallTemplateAdjuster(0.5),
                new RoundingTownHallTemplateAdjuster(5.0),
                new PercentTownHallTemplateAdjuster(-22.7),
            },
            new List<string>(),
            0.5,
            86302.3);
    }

    public double Evaluate(TownHallTemplate item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
