namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount draft calculation.</summary>
public interface IHeadcountDraftModifier
{
    string Describe();
    double Adjust(HeadcountDraft item, double valueValue);
}

public sealed record HeadcountDraftEndsAtOffsetAdjuster(double Weight) : IHeadcountDraftModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(HeadcountDraft item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record FlaggedHeadcountDraftAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHeadcountDraftAdjuster(double Percent) : IHeadcountDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount draft steps in order and keeps a trace of each.</summary>
public sealed class HeadcountDraftEngine(List<IHeadcountDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountDraftEngine DefaultHeadcountDraftEngine(Registry flags)
    {
        return new HeadcountDraftEngine(
            new List<IHeadcountDraftModifier>
            {
                new HeadcountDraftEndsAtOffsetAdjuster(0.24),
                new FlaggedHeadcountDraftAdjuster(flags, "headcount-draft-fallback", 5.8),
                new PercentHeadcountDraftAdjuster(-4.0),
            },
            new List<string>(),
            1.9,
            79522.4);
    }

    public double Run(HeadcountDraft item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
