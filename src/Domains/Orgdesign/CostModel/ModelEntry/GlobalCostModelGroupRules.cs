namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global cost model group must pass. Check returns its problems, or none.</summary>
public interface IGlobalCostModelGroupRule
{
    List<string> Check(GlobalCostModelGroup item);
}

public sealed record GlobalCostModelGroupChannelLengthRule(int MaxLength) : IGlobalCostModelGroupRule
{
    public List<string> Check(GlobalCostModelGroup item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalCostModelGroupCategoryLengthRule(int MaxLength) : IGlobalCostModelGroupRule
{
    public List<string> Check(GlobalCostModelGroup item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalCostModelGroupRequiredCodeRule(string Message) : IGlobalCostModelGroupRule
{
    public List<string> Check(GlobalCostModelGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalCostModelGroupAllowedNameRule(List<string> Allowed) : IGlobalCostModelGroupRule
{
    public List<string> Check(GlobalCostModelGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global cost model group.</summary>
public sealed record GlobalCostModelGroupValidator(List<IGlobalCostModelGroupRule> Rules)
{
    public List<string> Validate(GlobalCostModelGroup item)
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

    public void RequireValid(GlobalCostModelGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalCostModelGroup> items)
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

/// <summary>Functions of global cost model group rules.</summary>
public static class GlobalCostModelGroupRules
{
    public static GlobalCostModelGroupValidator DefaultGlobalCostModelGroupValidator()
    {
        return new GlobalCostModelGroupValidator(
            new List<IGlobalCostModelGroupRule>
            {
                new GlobalCostModelGroupChannelLengthRule(64),
                new GlobalCostModelGroupCategoryLengthRule(16),
                new GlobalCostModelGroupRequiredCodeRule("global cost model group needs this field"),
                new GlobalCostModelGroupAllowedNameRule(new List<string> { "adaptive" }),
            });
    }
}
