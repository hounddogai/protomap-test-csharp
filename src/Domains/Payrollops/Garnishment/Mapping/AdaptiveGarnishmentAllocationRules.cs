namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive garnishment allocation must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveGarnishmentAllocationRule
{
    List<string> Check(AdaptiveGarnishmentAllocation item);
}

public sealed record AdaptiveGarnishmentAllocationAllowedNameRule(List<string> Allowed) : IAdaptiveGarnishmentAllocationRule
{
    public List<string> Check(AdaptiveGarnishmentAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentAllocationRequiredCategoryRule(string Message) : IAdaptiveGarnishmentAllocationRule
{
    public List<string> Check(AdaptiveGarnishmentAllocation item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentAllocationRequiredNameRule(string Message) : IAdaptiveGarnishmentAllocationRule
{
    public List<string> Check(AdaptiveGarnishmentAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive garnishment allocation.</summary>
public sealed record AdaptiveGarnishmentAllocationValidator(List<IAdaptiveGarnishmentAllocationRule> Rules)
{
    public List<string> Validate(AdaptiveGarnishmentAllocation item)
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

    public void RequireValid(AdaptiveGarnishmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveGarnishmentAllocation> items)
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

/// <summary>Functions of adaptive garnishment allocation rules.</summary>
public static class AdaptiveGarnishmentAllocationRules
{
    public static AdaptiveGarnishmentAllocationValidator DefaultAdaptiveGarnishmentAllocationValidator()
    {
        return new AdaptiveGarnishmentAllocationValidator(
            new List<IAdaptiveGarnishmentAllocationRule>
            {
                new AdaptiveGarnishmentAllocationAllowedNameRule(new List<string> { "capped", "pending" }),
                new AdaptiveGarnishmentAllocationRequiredCategoryRule(
                    "adaptive garnishment allocation needs an entry"),
                new AdaptiveGarnishmentAllocationRequiredNameRule("adaptive garnishment allocation needs a value"),
            });
    }
}
