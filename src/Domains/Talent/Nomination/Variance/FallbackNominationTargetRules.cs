namespace Acme.Workforce.Talent.Nomination.Variance;

/// <summary>A rule a fallback nomination target must pass. Check returns its problems, or none.</summary>
public interface IFallbackNominationTargetRule
{
    List<string> Check(FallbackNominationTarget item);
}

public sealed record FallbackNominationTargetRequiredCodeRule(string Message) : IFallbackNominationTargetRule
{
    public List<string> Check(FallbackNominationTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationTargetRequiredSkillRule(string Message) : IFallbackNominationTargetRule
{
    public List<string> Check(FallbackNominationTarget item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationTargetCodeLengthRule(int MaxLength) : IFallbackNominationTargetRule
{
    public List<string> Check(FallbackNominationTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationTargetTitleLengthRule(int MaxLength) : IFallbackNominationTargetRule
{
    public List<string> Check(FallbackNominationTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback nomination target.</summary>
public sealed record FallbackNominationTargetValidator(List<IFallbackNominationTargetRule> Rules)
{
    public List<string> Validate(FallbackNominationTarget item)
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

    public void RequireValid(FallbackNominationTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackNominationTarget> items)
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

/// <summary>Functions of fallback nomination target rules.</summary>
public static class FallbackNominationTargetRules
{
    public static FallbackNominationTargetValidator DefaultFallbackNominationTargetValidator()
    {
        return new FallbackNominationTargetValidator(
            new List<IFallbackNominationTargetRule>
            {
                new FallbackNominationTargetRequiredCodeRule("fallback nomination target needs this field"),
                new FallbackNominationTargetRequiredSkillRule("fallback nomination target needs an entry"),
                new FallbackNominationTargetCodeLengthRule(255),
                new FallbackNominationTargetTitleLengthRule(120),
            });
    }
}
