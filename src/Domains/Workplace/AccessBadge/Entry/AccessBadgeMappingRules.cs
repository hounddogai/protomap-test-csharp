namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>A rule a access badge mapping must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeMappingRule
{
    List<string> Check(AccessBadgeMapping item);
}

public sealed record AccessBadgeMappingLabelLengthRule(int MaxLength) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMappingLimitRangeRule(int Min, int Max) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMappingNameLengthRule(int MaxLength) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMappingCodeLengthRule(int MaxLength) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMappingRequiredCodeRule(string Message) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMappingRequiredCategoryRule(string Message) : IAccessBadgeMappingRule
{
    public List<string> Check(AccessBadgeMapping item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge mapping.</summary>
public sealed record AccessBadgeMappingValidator(List<IAccessBadgeMappingRule> Rules)
{
    public List<string> Validate(AccessBadgeMapping item)
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

    public void RequireValid(AccessBadgeMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeMapping> items)
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

/// <summary>Functions of access badge mapping rules.</summary>
public static class AccessBadgeMappingRules
{
    public static AccessBadgeMappingValidator DefaultAccessBadgeMappingValidator()
    {
        return new AccessBadgeMappingValidator(
            new List<IAccessBadgeMappingRule>
            {
                new AccessBadgeMappingLabelLengthRule(3600),
                new AccessBadgeMappingLimitRangeRule(64, 86400),
                new AccessBadgeMappingNameLengthRule(255),
                new AccessBadgeMappingCodeLengthRule(120),
                new AccessBadgeMappingRequiredCodeRule("access badge mapping needs this field"),
                new AccessBadgeMappingRequiredCategoryRule("access badge mapping needs a value"),
            });
    }
}
