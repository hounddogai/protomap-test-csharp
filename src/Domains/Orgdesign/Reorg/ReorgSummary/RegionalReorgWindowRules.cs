namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>A rule a regional reorg window must pass. Check returns its problems, or none.</summary>
public interface IRegionalReorgWindowRule
{
    List<string> Check(RegionalReorgWindow item);
}

public sealed record RegionalReorgWindowBadgeLengthRule(int MaxLength) : IRegionalReorgWindowRule
{
    public List<string> Check(RegionalReorgWindow item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgWindowUpdatedAtRangeRule(int Min, int Max) : IRegionalReorgWindowRule
{
    public List<string> Check(RegionalReorgWindow item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalReorgWindowRequiredCodeRule(string Message) : IRegionalReorgWindowRule
{
    public List<string> Check(RegionalReorgWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgWindowCodeLengthRule(int MaxLength) : IRegionalReorgWindowRule
{
    public List<string> Check(RegionalReorgWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgWindowRequiredCategoryRule(string Message) : IRegionalReorgWindowRule
{
    public List<string> Check(RegionalReorgWindow item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional reorg window.</summary>
public sealed record RegionalReorgWindowValidator(List<IRegionalReorgWindowRule> Rules)
{
    public List<string> Validate(RegionalReorgWindow item)
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

    public void RequireValid(RegionalReorgWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalReorgWindow> items)
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

/// <summary>Functions of regional reorg window rules.</summary>
public static class RegionalReorgWindowRules
{
    public static RegionalReorgWindowValidator DefaultRegionalReorgWindowValidator()
    {
        return new RegionalReorgWindowValidator(
            new List<IRegionalReorgWindowRule>
            {
                new RegionalReorgWindowBadgeLengthRule(120),
                new RegionalReorgWindowUpdatedAtRangeRule(16, 120),
                new RegionalReorgWindowRequiredCodeRule("regional reorg window needs this field"),
                new RegionalReorgWindowCodeLengthRule(32),
                new RegionalReorgWindowRequiredCategoryRule("regional reorg window needs this field"),
            });
    }
}
