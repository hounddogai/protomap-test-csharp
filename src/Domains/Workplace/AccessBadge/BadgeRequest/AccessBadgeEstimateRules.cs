namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge estimate must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeEstimateRule
{
    List<string> Check(AccessBadgeEstimate item);
}

public sealed record AccessBadgeEstimateCeilingRangeRule(double Min, double Max) : IAccessBadgeEstimateRule
{
    public List<string> Check(AccessBadgeEstimate item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEstimateCodeLengthRule(int MaxLength) : IAccessBadgeEstimateRule
{
    public List<string> Check(AccessBadgeEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEstimateMarginRangeRule(double Min, double Max) : IAccessBadgeEstimateRule
{
    public List<string> Check(AccessBadgeEstimate item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEstimateTitleLengthRule(int MaxLength) : IAccessBadgeEstimateRule
{
    public List<string> Check(AccessBadgeEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEstimateAllowedTitleRule(List<string> Allowed) : IAccessBadgeEstimateRule
{
    public List<string> Check(AccessBadgeEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge estimate.</summary>
public sealed record AccessBadgeEstimateValidator(List<IAccessBadgeEstimateRule> Rules)
{
    public List<string> Validate(AccessBadgeEstimate item)
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

    public void RequireValid(AccessBadgeEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeEstimate> items)
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

/// <summary>Functions of access badge estimate rules.</summary>
public static class AccessBadgeEstimateRules
{
    public static AccessBadgeEstimateValidator DefaultAccessBadgeEstimateValidator()
    {
        return new AccessBadgeEstimateValidator(
            new List<IAccessBadgeEstimateRule>
            {
                new AccessBadgeEstimateCeilingRangeRule(0.2, 3846.7),
                new AccessBadgeEstimateCodeLengthRule(255),
                new AccessBadgeEstimateMarginRangeRule(4.6, 1266.7),
                new AccessBadgeEstimateTitleLengthRule(32),
                new AccessBadgeEstimateAllowedTitleRule(new List<string> { "global", "lenient", "fallback" }),
            });
    }
}
