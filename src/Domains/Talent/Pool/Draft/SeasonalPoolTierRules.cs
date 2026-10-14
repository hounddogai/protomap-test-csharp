namespace Acme.Workforce.Talent.Pool.Draft;

/// <summary>A rule a seasonal pool tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPoolTierRule
{
    List<string> Check(SeasonalPoolTier item);
}

public sealed record SeasonalPoolTierRequiredReviewerRule(string Message) : ISeasonalPoolTierRule
{
    public List<string> Check(SeasonalPoolTier item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPoolTierNameLengthRule(int MaxLength) : ISeasonalPoolTierRule
{
    public List<string> Check(SeasonalPoolTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPoolTierRequiredNameRule(string Message) : ISeasonalPoolTierRule
{
    public List<string> Check(SeasonalPoolTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pool tier.</summary>
public sealed record SeasonalPoolTierValidator(List<ISeasonalPoolTierRule> Rules)
{
    public List<string> Validate(SeasonalPoolTier item)
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

    public void RequireValid(SeasonalPoolTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPoolTier> items)
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

/// <summary>Functions of seasonal pool tier rules.</summary>
public static class SeasonalPoolTierRules
{
    public static SeasonalPoolTierValidator DefaultSeasonalPoolTierValidator()
    {
        return new SeasonalPoolTierValidator(
            new List<ISeasonalPoolTierRule>
            {
                new SeasonalPoolTierRequiredReviewerRule("seasonal pool tier needs a value"),
                new SeasonalPoolTierNameLengthRule(120),
                new SeasonalPoolTierRequiredNameRule("seasonal pool tier needs an entry"),
            });
    }
}
