namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal accrual template calculation.</summary>
public interface ISeasonalAccrualTemplateAdjuster
{
    string Describe();
    double Adjust(SeasonalAccrualTemplate item, double valueValue);
}

public sealed record SeasonalAccrualTemplateGraceDaysOffsetAdjuster(double Weight) : ISeasonalAccrualTemplateAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SeasonalAccrualTemplate item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record CapSeasonalAccrualTemplateAdjuster(double Cap) : ISeasonalAccrualTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAccrualTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalAccrualTemplateAdjuster(double Step) : ISeasonalAccrualTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAccrualTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSeasonalAccrualTemplateAdjuster(double Percent) : ISeasonalAccrualTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalAccrualTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalAccrualTemplateAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAccrualTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAccrualTemplate item, double valueValue)
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

/// <summary>Runs the seasonal accrual template steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAccrualTemplateEngine(List<ISeasonalAccrualTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAccrualTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAccrualTemplateEngine DefaultSeasonalAccrualTemplateEngine()
    {
        return new SeasonalAccrualTemplateEngine(
            new List<ISeasonalAccrualTemplateAdjuster>
            {
                new SeasonalAccrualTemplateGraceDaysOffsetAdjuster(1.19),
                new CapSeasonalAccrualTemplateAdjuster(26370.0),
                new RoundingSeasonalAccrualTemplateAdjuster(0.1),
                new PercentSeasonalAccrualTemplateAdjuster(-14.1),
                new TieredSeasonalAccrualTemplateAdjuster(
                    new List<double> { 386.0, 5416.0, 6303.0 },
                    new List<double> { 1.11, 0.82, 1.07 }),
            },
            new List<string>(),
            4.9,
            21729.8);
    }

    public double Evaluate(SeasonalAccrualTemplate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAccrualTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
