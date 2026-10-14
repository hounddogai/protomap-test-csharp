namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>One step of the retro pay template calculation.</summary>
public interface IRetroPayTemplateAdjuster
{
    string Describe();
    double Adjust(RetroPayTemplate item, double valueValue);
}

public sealed record RetroPayTemplateAmountOffsetAdjuster(double Weight) : IRetroPayTemplateAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RetroPayTemplate item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredRetroPayTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayTemplate item, double valueValue)
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

public sealed record PercentRetroPayTemplateAdjuster(double Percent) : IRetroPayTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRetroPayTemplateAdjuster(double Minimum) : IRetroPayTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the retro pay template steps in order and keeps a trace of each.</summary>
public sealed class RetroPayTemplateEngine(List<IRetroPayTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayTemplateEngine DefaultRetroPayTemplateEngine()
    {
        return new RetroPayTemplateEngine(
            new List<IRetroPayTemplateAdjuster>
            {
                new RetroPayTemplateAmountOffsetAdjuster(1.59),
                new TieredRetroPayTemplateAdjuster(
                    new List<double> { 4274.0, 6684.0 },
                    new List<double> { 1.03, 0.98 }),
                new PercentRetroPayTemplateAdjuster(-23.9),
                new MinimumRetroPayTemplateAdjuster(17.7),
            },
            new List<string>(),
            2.0,
            30226.5);
    }

    public double Evaluate(RetroPayTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
