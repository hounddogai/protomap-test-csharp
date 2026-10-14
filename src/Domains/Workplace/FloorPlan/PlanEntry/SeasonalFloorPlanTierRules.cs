namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal floor plan tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanTierRule
{
    List<string> Check(SeasonalFloorPlanTier item);
}

public sealed record SeasonalFloorPlanTierTitleLengthRule(int MaxLength) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanTierCodePrefixRule(string Prefix) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanTierWeightRangeRule(double Min, double Max) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanTierAllowedCategoryRule(List<string> Allowed) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanTierRequiredCodeRule(string Message) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanTierCodeLengthRule(int MaxLength) : ISeasonalFloorPlanTierRule
{
    public List<string> Check(SeasonalFloorPlanTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan tier.</summary>
public sealed record SeasonalFloorPlanTierValidator(List<ISeasonalFloorPlanTierRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanTier item)
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

    public void RequireValid(SeasonalFloorPlanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanTier> items)
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

/// <summary>Functions of seasonal floor plan tier rules.</summary>
public static class SeasonalFloorPlanTierRules
{
    public static SeasonalFloorPlanTierValidator DefaultSeasonalFloorPlanTierValidator()
    {
        return new SeasonalFloorPlanTierValidator(
            new List<ISeasonalFloorPlanTierRule>
            {
                new SeasonalFloorPlanTierTitleLengthRule(64),
                new SeasonalFloorPlanTierCodePrefixRule("REF"),
                new SeasonalFloorPlanTierWeightRangeRule(5.6, 4216.9),
                new SeasonalFloorPlanTierAllowedCategoryRule(new List<string> { "tiered", "fallback" }),
                new SeasonalFloorPlanTierRequiredCodeRule("seasonal floor plan tier needs an entry"),
                new SeasonalFloorPlanTierCodeLengthRule(120),
            });
    }
}
