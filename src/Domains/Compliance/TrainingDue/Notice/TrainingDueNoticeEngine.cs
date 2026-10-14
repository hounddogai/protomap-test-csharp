namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due notice calculation.</summary>
public interface ITrainingDueNoticeStep
{
    string Describe();
    double Adjust(TrainingDueNotice item, double valueValue);
}

public sealed record TieredTrainingDueNoticeAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
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

public sealed record LabelTrainingDueNoticeAdjuster(Dictionary<string, double> Factors) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTrainingDueNoticeAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueNoticeGraceDaysOffsetAdjuster(double Weight) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentTrainingDueNoticeAdjuster(double Percent) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTrainingDueNoticeAdjuster(double Step) : ITrainingDueNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the training due notice steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueNoticeEngine(List<ITrainingDueNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueNoticeEngine DefaultTrainingDueNoticeEngine(Registry flags)
    {
        return new TrainingDueNoticeEngine(
            new List<ITrainingDueNoticeStep>
            {
                new TieredTrainingDueNoticeAdjuster(
                    new List<double> { 1512.0, 9820.0 },
                    new List<double> { 1.1, 1.02 }),
                new LabelTrainingDueNoticeAdjuster(new Dictionary<string, double> { ["central"] = 1.35 }),
                new FlaggedTrainingDueNoticeAdjuster(flags, "training-due-notice-strict", 7.0),
                new TrainingDueNoticeGraceDaysOffsetAdjuster(0.03),
                new PercentTrainingDueNoticeAdjuster(18.7),
                new RoundingTrainingDueNoticeAdjuster(0.5),
            },
            new List<string>(),
            3.5,
            4344.8);
    }

    public double Evaluate(TrainingDueNotice item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
