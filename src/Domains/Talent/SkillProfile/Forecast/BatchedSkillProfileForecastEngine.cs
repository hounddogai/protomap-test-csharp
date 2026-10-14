namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched skill profile forecast calculation.</summary>
public interface IBatchedSkillProfileForecastModifier
{
    string Describe();
    double Adjust(BatchedSkillProfileForecast item, double valueValue);
}

public sealed record BatchedSkillProfileForecastAmountOffsetAdjuster(double Weight) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapBatchedSkillProfileForecastAdjuster(double Cap) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedSkillProfileForecastAdjuster(List<double> Bounds, List<double> Rates) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
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

public sealed record PercentBatchedSkillProfileForecastAdjuster(double Percent) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameBatchedSkillProfileForecastAdjuster(Dictionary<string, double> Factors) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBatchedSkillProfileForecastAdjuster(double Step) : IBatchedSkillProfileForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedSkillProfileForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched skill profile forecast steps in order and keeps a trace of each.</summary>
public sealed class BatchedSkillProfileForecastEngine(List<IBatchedSkillProfileForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSkillProfileForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSkillProfileForecastEngine DefaultBatchedSkillProfileForecastEngine()
    {
        return new BatchedSkillProfileForecastEngine(
            new List<IBatchedSkillProfileForecastModifier>
            {
                new BatchedSkillProfileForecastAmountOffsetAdjuster(0.22),
                new CapBatchedSkillProfileForecastAdjuster(24858.0),
                new TieredBatchedSkillProfileForecastAdjuster(
                    new List<double> { 2343.0, 3770.0 },
                    new List<double> { 0.83, 0.83 }),
                new PercentBatchedSkillProfileForecastAdjuster(-8.5),
                new NameBatchedSkillProfileForecastAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.85, ["central"] = 0.97, ["south"] = 1.05 }),
                new RoundingBatchedSkillProfileForecastAdjuster(0.25),
            },
            new List<string>(),
            4.4,
            58497.9);
    }

    public double Price(BatchedSkillProfileForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSkillProfileForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
