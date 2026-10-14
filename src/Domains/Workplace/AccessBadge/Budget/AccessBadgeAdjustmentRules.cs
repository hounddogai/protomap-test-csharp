namespace Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>A rule a access badge adjustment must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeAdjustmentRule
{
    List<string> Check(AccessBadgeAdjustment item);
}

public sealed record AccessBadgeAdjustmentCountryLengthRule(int MaxLength) : IAccessBadgeAdjustmentRule
{
    public List<string> Check(AccessBadgeAdjustment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAdjustmentScoreRangeRule(double Min, double Max) : IAccessBadgeAdjustmentRule
{
    public List<string> Check(AccessBadgeAdjustment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeAdjustmentTitleLengthRule(int MaxLength) : IAccessBadgeAdjustmentRule
{
    public List<string> Check(AccessBadgeAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAdjustmentCodePrefixRule(string Prefix) : IAccessBadgeAdjustmentRule
{
    public List<string> Check(AccessBadgeAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge adjustment.</summary>
public sealed record AccessBadgeAdjustmentValidator(List<IAccessBadgeAdjustmentRule> Rules)
{
    public List<string> Validate(AccessBadgeAdjustment item)
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

    public void RequireValid(AccessBadgeAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeAdjustment> items)
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

/// <summary>Functions of access badge adjustment rules.</summary>
public static class AccessBadgeAdjustmentRules
{
    public static AccessBadgeAdjustmentValidator DefaultAccessBadgeAdjustmentValidator()
    {
        return new AccessBadgeAdjustmentValidator(
            new List<IAccessBadgeAdjustmentRule>
            {
                new AccessBadgeAdjustmentCountryLengthRule(120),
                new AccessBadgeAdjustmentScoreRangeRule(6.2, 3344.2),
                new AccessBadgeAdjustmentTitleLengthRule(86400),
                new AccessBadgeAdjustmentCodePrefixRule("REF"),
            });
    }
}
