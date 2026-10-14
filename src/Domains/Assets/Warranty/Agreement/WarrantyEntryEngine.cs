namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the warranty entry calculation.</summary>
public interface IWarrantyEntryStep
{
    string Describe();
    double Adjust(WarrantyEntry item, double valueValue);
}

public sealed record FlaggedWarrantyEntryAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WarrantyEntryAmountOffsetAdjuster(double Weight) : IWarrantyEntryStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(WarrantyEntry item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumWarrantyEntryAdjuster(double Minimum) : IWarrantyEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWarrantyEntryAdjuster(double Percent) : IWarrantyEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceWarrantyEntryAdjuster(Dictionary<string, double> Factors) : IWarrantyEntryStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(WarrantyEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the warranty entry steps in order and keeps a trace of each.</summary>
public sealed class WarrantyEntryEngine(List<IWarrantyEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyEntryEngine DefaultWarrantyEntryEngine(Registry flags)
    {
        return new WarrantyEntryEngine(
            new List<IWarrantyEntryStep>
            {
                new FlaggedWarrantyEntryAdjuster(flags, "warranty-entry-global", -0.9),
                new WarrantyEntryAmountOffsetAdjuster(1.74),
                new MinimumWarrantyEntryAdjuster(24.9),
                new PercentWarrantyEntryAdjuster(-3.1),
                new SourceWarrantyEntryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.03, ["west"] = 0.75 }),
            },
            new List<string>(),
            3.7,
            52190.0);
    }

    public double Price(WarrantyEntry item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
