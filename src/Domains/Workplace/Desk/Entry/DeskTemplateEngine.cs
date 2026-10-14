namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk template calculation.</summary>
public interface IDeskTemplateModifier
{
    string Describe();
    double Adjust(DeskTemplate item, double valueValue);
}

public sealed record CountryDeskTemplateAdjuster(Dictionary<string, double> Factors) : IDeskTemplateModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DeskTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDeskTemplateAdjuster(double Step) : IDeskTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDeskTemplateAdjuster(double Minimum) : IDeskTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DeskTemplateRetriesOffsetAdjuster(double Weight) : IDeskTemplateModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(DeskTemplate item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the desk template steps in order and keeps a trace of each.</summary>
public sealed class DeskTemplateEngine(List<IDeskTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskTemplateEngine DefaultDeskTemplateEngine()
    {
        return new DeskTemplateEngine(
            new List<IDeskTemplateModifier>
            {
                new CountryDeskTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.97, ["retail"] = 1.08 }),
                new RoundingDeskTemplateAdjuster(0.25),
                new MinimumDeskTemplateAdjuster(17.8),
                new DeskTemplateRetriesOffsetAdjuster(1.79),
            },
            new List<string>(),
            0.9,
            2009.6);
    }

    public double Run(DeskTemplate item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeskTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
