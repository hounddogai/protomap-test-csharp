namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback pulse survey segment must pass. Check returns its problems, or none.</summary>
public interface IFallbackPulseSurveySegmentRule
{
    List<string> Check(FallbackPulseSurveySegment item);
}

public sealed record FallbackPulseSurveySegmentDurationMinutesRangeRule(int Min, int Max) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPulseSurveySegmentAllowedGradeRule(List<string> Allowed) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPulseSurveySegmentRequiredGradeRule(string Message) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPulseSurveySegmentCodePrefixRule(string Prefix) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPulseSurveySegmentRequiredCodeRule(string Message) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPulseSurveySegmentRatingRangeRule(double Min, double Max) : IFallbackPulseSurveySegmentRule
{
    public List<string> Check(FallbackPulseSurveySegment item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pulse survey segment.</summary>
public sealed record FallbackPulseSurveySegmentValidator(List<IFallbackPulseSurveySegmentRule> Rules)
{
    public List<string> Validate(FallbackPulseSurveySegment item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(FallbackPulseSurveySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPulseSurveySegment> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of fallback pulse survey segment rules.</summary>
public static class FallbackPulseSurveySegmentRules
{
    public static FallbackPulseSurveySegmentValidator DefaultFallbackPulseSurveySegmentValidator()
    {
        return new FallbackPulseSurveySegmentValidator(
            new List<IFallbackPulseSurveySegmentRule>
            {
                new FallbackPulseSurveySegmentDurationMinutesRangeRule(16, 86400),
                new FallbackPulseSurveySegmentAllowedGradeRule(new List<string> { "capped" }),
                new FallbackPulseSurveySegmentRequiredGradeRule("fallback pulse survey segment needs an entry"),
                new FallbackPulseSurveySegmentCodePrefixRule("X"),
                new FallbackPulseSurveySegmentRequiredCodeRule("fallback pulse survey segment needs this field"),
                new FallbackPulseSurveySegmentRatingRangeRule(5.1, 3601.7),
            });
    }
}
