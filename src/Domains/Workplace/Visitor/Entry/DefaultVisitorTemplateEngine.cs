namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default visitor template calculation.</summary>
public interface IDefaultVisitorTemplateStep
{
    string Describe();
    double Adjust(DefaultVisitorTemplate item, double valueValue);
}

public sealed record CountryDefaultVisitorTemplateAdjuster(Dictionary<string, double> Factors) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedDefaultVisitorTemplateAdjuster(Registry Flags, string Flag, double Percent) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultVisitorTemplateSequenceOffsetAdjuster(double Weight) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record MinimumDefaultVisitorTemplateAdjuster(double Minimum) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultVisitorTemplateAdjuster(double Cap) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultVisitorTemplateAdjuster(List<double> Bounds, List<double> Rates) : IDefaultVisitorTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultVisitorTemplate item, double valueValue)
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

/// <summary>Runs the default visitor template steps in order and keeps a trace of each.</summary>
public sealed class DefaultVisitorTemplateEngine(List<IDefaultVisitorTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultVisitorTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultVisitorTemplateEngine DefaultDefaultVisitorTemplateEngine(Registry flags)
    {
        return new DefaultVisitorTemplateEngine(
            new List<IDefaultVisitorTemplateStep>
            {
                new CountryDefaultVisitorTemplateAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.75, ["east"] = 1.3, ["central"] = 1.13 }),
                new FlaggedDefaultVisitorTemplateAdjuster(flags, "default-visitor-template-legacy", 0.1),
                new DefaultVisitorTemplateSequenceOffsetAdjuster(0.8),
                new MinimumDefaultVisitorTemplateAdjuster(1.8),
                new CapDefaultVisitorTemplateAdjuster(7896.0),
                new TieredDefaultVisitorTemplateAdjuster(
                    new List<double> { 5458.0, 6593.0, 9324.0 },
                    new List<double> { 1.14, 1.12, 1.09 }),
            },
            new List<string>(),
            4.6,
            10140.0);
    }

    public double Run(DefaultVisitorTemplate item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultVisitorTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
