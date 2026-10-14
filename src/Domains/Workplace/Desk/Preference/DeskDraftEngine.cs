namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the desk draft calculation.</summary>
public interface IDeskDraftFactor
{
    string Describe();
    double Adjust(DeskDraft item, double valueValue);
}

public sealed record FlaggedDeskDraftAdjuster(Registry Flags, string Flag, double Percent) : IDeskDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeskDraftAdjuster(double Percent) : IDeskDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDeskDraftAdjuster(Dictionary<string, double> Factors) : IDeskDraftFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DeskDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record DeskDraftBatchSizeOffsetAdjuster(double Weight) : IDeskDraftFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(DeskDraft item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredDeskDraftAdjuster(List<double> Bounds, List<double> Rates) : IDeskDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskDraft item, double valueValue)
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

/// <summary>Runs the desk draft steps in order and keeps a trace of each.</summary>
public sealed class DeskDraftEngine(List<IDeskDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskDraftEngine DefaultDeskDraftEngine(Registry flags)
    {
        return new DeskDraftEngine(
            new List<IDeskDraftFactor>
            {
                new FlaggedDeskDraftAdjuster(flags, "desk-draft-legacy", 4.9),
                new PercentDeskDraftAdjuster(20.3),
                new TitleDeskDraftAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.34, ["east"] = 1.02, ["west"] = 1.1 }),
                new DeskDraftBatchSizeOffsetAdjuster(1.73),
                new TieredDeskDraftAdjuster(
                    new List<double> { 1290.0, 1711.0, 6262.0 },
                    new List<double> { 1.06, 1.12, 0.82 }),
            },
            new List<string>(),
            2.1,
            58909.5);
    }

    public double Price(DeskDraft item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
