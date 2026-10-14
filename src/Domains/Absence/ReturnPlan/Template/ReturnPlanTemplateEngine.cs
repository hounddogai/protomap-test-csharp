namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan template calculation.</summary>
public interface IReturnPlanTemplateModifier
{
    string Describe();
    double Adjust(ReturnPlanTemplate item, double valueValue);
}

public sealed record CategoryReturnPlanTemplateAdjuster(Dictionary<string, double> Factors) : IReturnPlanTemplateModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ReturnPlanTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapReturnPlanTemplateAdjuster(double Cap) : IReturnPlanTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentReturnPlanTemplateAdjuster(double Percent) : IReturnPlanTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan template steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanTemplateEngine(List<IReturnPlanTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanTemplateEngine DefaultReturnPlanTemplateEngine()
    {
        return new ReturnPlanTemplateEngine(
            new List<IReturnPlanTemplateModifier>
            {
                new CategoryReturnPlanTemplateAdjuster(new Dictionary<string, double> { ["retail"] = 0.73 }),
                new CapReturnPlanTemplateAdjuster(44797.0),
                new PercentReturnPlanTemplateAdjuster(7.5),
            },
            new List<string>(),
            3.4,
            3208.5);
    }

    public double Price(ReturnPlanTemplate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
