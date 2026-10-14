namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge template must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeTemplateRule
{
    List<string> Check(AccessBadgeTemplate item);
}

public sealed record AccessBadgeTemplateCodePrefixRule(string Prefix) : IAccessBadgeTemplateRule
{
    public List<string> Check(AccessBadgeTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTemplateAmountRangeRule(double Min, double Max) : IAccessBadgeTemplateRule
{
    public List<string> Check(AccessBadgeTemplate item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTemplateRequiredCodeRule(string Message) : IAccessBadgeTemplateRule
{
    public List<string> Check(AccessBadgeTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTemplateAllowedCodeRule(List<string> Allowed) : IAccessBadgeTemplateRule
{
    public List<string> Check(AccessBadgeTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge template.</summary>
public sealed record AccessBadgeTemplateValidator(List<IAccessBadgeTemplateRule> Rules)
{
    public List<string> Validate(AccessBadgeTemplate item)
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

    public void RequireValid(AccessBadgeTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeTemplate> items)
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

/// <summary>Functions of access badge template rules.</summary>
public static class AccessBadgeTemplateRules
{
    public static AccessBadgeTemplateValidator DefaultAccessBadgeTemplateValidator()
    {
        return new AccessBadgeTemplateValidator(
            new List<IAccessBadgeTemplateRule>
            {
                new AccessBadgeTemplateCodePrefixRule("X"),
                new AccessBadgeTemplateAmountRangeRule(9.0, 3805.5),
                new AccessBadgeTemplateRequiredCodeRule("access badge template needs this field"),
                new AccessBadgeTemplateAllowedCodeRule(new List<string> { "default", "global", "seasonal" }),
            });
    }
}
