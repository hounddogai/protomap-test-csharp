namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback span score must pass. Check returns its problems, or none.</summary>
public interface IFallbackSpanScoreRule
{
    List<string> Check(FallbackSpanScore item);
}

public sealed record FallbackSpanScoreRequiredReviewerRule(string Message) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSpanScoreRegionLengthRule(int MaxLength) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSpanScoreNameLengthRule(int MaxLength) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSpanScoreRequiredNameRule(string Message) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSpanScoreAllowedReviewerRule(List<string> Allowed) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSpanScoreGraceDaysRangeRule(int Min, int Max) : IFallbackSpanScoreRule
{
    public List<string> Check(FallbackSpanScore item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback span score.</summary>
public sealed record FallbackSpanScoreValidator(List<IFallbackSpanScoreRule> Rules)
{
    public List<string> Validate(FallbackSpanScore item)
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

    public void RequireValid(FallbackSpanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSpanScore> items)
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

/// <summary>Functions of fallback span score rules.</summary>
public static class FallbackSpanScoreRules
{
    public static FallbackSpanScoreValidator DefaultFallbackSpanScoreValidator()
    {
        return new FallbackSpanScoreValidator(
            new List<IFallbackSpanScoreRule>
            {
                new FallbackSpanScoreRequiredReviewerRule("fallback span score needs a value"),
                new FallbackSpanScoreRegionLengthRule(86400),
                new FallbackSpanScoreNameLengthRule(3600),
                new FallbackSpanScoreRequiredNameRule("fallback span score needs this field"),
                new FallbackSpanScoreAllowedReviewerRule(new List<string> { "global", "scheduled" }),
                new FallbackSpanScoreGraceDaysRangeRule(255, 86400),
            });
    }
}
