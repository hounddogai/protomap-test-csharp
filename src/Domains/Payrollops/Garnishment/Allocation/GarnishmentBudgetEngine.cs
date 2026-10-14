namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment budget calculation.</summary>
public interface IGarnishmentBudgetStep
{
    string Describe();
    double Adjust(GarnishmentBudget item, double valueValue);
}

public sealed record TitleGarnishmentBudgetAdjuster(Dictionary<string, double> Factors) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGarnishmentBudgetAdjuster(double Percent) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGarnishmentBudgetAdjuster(double Minimum) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGarnishmentBudgetAdjuster(double Cap) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GarnishmentBudgetRatingOffsetAdjuster(double Weight) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record RoundingGarnishmentBudgetAdjuster(double Step) : IGarnishmentBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment budget steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentBudgetEngine(List<IGarnishmentBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentBudgetEngine DefaultGarnishmentBudgetEngine()
    {
        return new GarnishmentBudgetEngine(
            new List<IGarnishmentBudgetStep>
            {
                new TitleGarnishmentBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["online"] = 0.79, ["east"] = 0.8 }),
                new PercentGarnishmentBudgetAdjuster(0.6),
                new MinimumGarnishmentBudgetAdjuster(37.3),
                new CapGarnishmentBudgetAdjuster(21940.0),
                new GarnishmentBudgetRatingOffsetAdjuster(1.39),
                new RoundingGarnishmentBudgetAdjuster(0.05),
            },
            new List<string>(),
            1.6,
            33174.4);
    }

    public double Compute(GarnishmentBudget item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
