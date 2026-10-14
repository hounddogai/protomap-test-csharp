namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile review calculation.</summary>
public interface ISkillProfileReviewAdjuster
{
    string Describe();
    double Adjust(SkillProfileReview item, double valueValue);
}

public sealed record MinimumSkillProfileReviewAdjuster(double Minimum) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentSkillProfileReviewAdjuster(Dictionary<string, double> Factors) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SkillProfileReviewAmountOffsetAdjuster(double Weight) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingSkillProfileReviewAdjuster(double Step) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSkillProfileReviewAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSkillProfileReviewAdjuster(double Cap) : ISkillProfileReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the skill profile review steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileReviewEngine(List<ISkillProfileReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileReviewEngine DefaultSkillProfileReviewEngine(Registry flags)
    {
        return new SkillProfileReviewEngine(
            new List<ISkillProfileReviewAdjuster>
            {
                new MinimumSkillProfileReviewAdjuster(10.4),
                new DepartmentSkillProfileReviewAdjuster(new Dictionary<string, double> { ["north"] = 0.76 }),
                new SkillProfileReviewAmountOffsetAdjuster(1.63),
                new RoundingSkillProfileReviewAdjuster(5.0),
                new FlaggedSkillProfileReviewAdjuster(flags, "skill-profile-review-default", -4.5),
                new CapSkillProfileReviewAdjuster(2888.0),
            },
            new List<string>(),
            0.5,
            12194.9);
    }

    public double Compute(SkillProfileReview item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
