namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive reorg budget must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveReorgBudgetRule
{
    List<string> Check(AdaptiveReorgBudget item);
}

public sealed record AdaptiveReorgBudgetCodeLengthRule(int MaxLength) : IAdaptiveReorgBudgetRule
{
    public List<string> Check(AdaptiveReorgBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBudgetCodePrefixRule(string Prefix) : IAdaptiveReorgBudgetRule
{
    public List<string> Check(AdaptiveReorgBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBudgetRequiredCategoryRule(string Message) : IAdaptiveReorgBudgetRule
{
    public List<string> Check(AdaptiveReorgBudget item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBudgetAllowedDescriptionRule(List<string> Allowed) : IAdaptiveReorgBudgetRule
{
    public List<string> Check(AdaptiveReorgBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBudgetRequiredDescriptionRule(string Message) : IAdaptiveReorgBudgetRule
{
    public List<string> Check(AdaptiveReorgBudget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive reorg budget.</summary>
public sealed record AdaptiveReorgBudgetValidator(List<IAdaptiveReorgBudgetRule> Rules)
{
    public List<string> Validate(AdaptiveReorgBudget item)
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

    public void RequireValid(AdaptiveReorgBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveReorgBudget> items)
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

/// <summary>Functions of adaptive reorg budget rules.</summary>
public static class AdaptiveReorgBudgetRules
{
    public static AdaptiveReorgBudgetValidator DefaultAdaptiveReorgBudgetValidator()
    {
        return new AdaptiveReorgBudgetValidator(
            new List<IAdaptiveReorgBudgetRule>
            {
                new AdaptiveReorgBudgetCodeLengthRule(16),
                new AdaptiveReorgBudgetCodePrefixRule("X"),
                new AdaptiveReorgBudgetRequiredCategoryRule("adaptive reorg budget needs this field"),
                new AdaptiveReorgBudgetAllowedDescriptionRule(new List<string> { "pending", "global" }),
                new AdaptiveReorgBudgetRequiredDescriptionRule("adaptive reorg budget needs a value"),
            });
    }
}
