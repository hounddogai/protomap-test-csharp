namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>A rule a seasonal access badge cycle must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccessBadgeCycleRule
{
    List<string> Check(SeasonalAccessBadgeCycle item);
}

public sealed record SeasonalAccessBadgeCycleCategoryLengthRule(int MaxLength) : ISeasonalAccessBadgeCycleRule
{
    public List<string> Check(SeasonalAccessBadgeCycle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeCycleTitleLengthRule(int MaxLength) : ISeasonalAccessBadgeCycleRule
{
    public List<string> Check(SeasonalAccessBadgeCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeCyclePriorityRangeRule(int Min, int Max) : ISeasonalAccessBadgeCycleRule
{
    public List<string> Check(SeasonalAccessBadgeCycle item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeCycleRequiredCodeRule(string Message) : ISeasonalAccessBadgeCycleRule
{
    public List<string> Check(SeasonalAccessBadgeCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeCycleCodePrefixRule(string Prefix) : ISeasonalAccessBadgeCycleRule
{
    public List<string> Check(SeasonalAccessBadgeCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal access badge cycle.</summary>
public sealed record SeasonalAccessBadgeCycleValidator(List<ISeasonalAccessBadgeCycleRule> Rules)
{
    public List<string> Validate(SeasonalAccessBadgeCycle item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(SeasonalAccessBadgeCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccessBadgeCycle> items)
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

/// <summary>Functions of seasonal access badge cycle rules.</summary>
public static class SeasonalAccessBadgeCycleRules
{
    public static SeasonalAccessBadgeCycleValidator DefaultSeasonalAccessBadgeCycleValidator()
    {
        return new SeasonalAccessBadgeCycleValidator(
            new List<ISeasonalAccessBadgeCycleRule>
            {
                new SeasonalAccessBadgeCycleCategoryLengthRule(1000),
                new SeasonalAccessBadgeCycleTitleLengthRule(120),
                new SeasonalAccessBadgeCyclePriorityRangeRule(32, 1000),
                new SeasonalAccessBadgeCycleRequiredCodeRule("seasonal access badge cycle needs this field"),
                new SeasonalAccessBadgeCycleCodePrefixRule("Q"),
            });
    }
}
