namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model policy must pass. Check returns its problems, or none.</summary>
public interface ICostModelPolicyRule
{
    List<string> Check(CostModelPolicy item);
}

public sealed record CostModelPolicyRequiredUnitRule(string Message) : ICostModelPolicyRule
{
    public List<string> Check(CostModelPolicy item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelPolicyEndsAtRangeRule(int Min, int Max) : ICostModelPolicyRule
{
    public List<string> Check(CostModelPolicy item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelPolicyAllowedCodeRule(List<string> Allowed) : ICostModelPolicyRule
{
    public List<string> Check(CostModelPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record CostModelPolicyUnitLengthRule(int MaxLength) : ICostModelPolicyRule
{
    public List<string> Check(CostModelPolicy item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model policy.</summary>
public sealed record CostModelPolicyValidator(List<ICostModelPolicyRule> Rules)
{
    public List<string> Validate(CostModelPolicy item)
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

    public void RequireValid(CostModelPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelPolicy> items)
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

/// <summary>Functions of cost model policy rules.</summary>
public static class CostModelPolicyRules
{
    public static CostModelPolicyValidator DefaultCostModelPolicyValidator()
    {
        return new CostModelPolicyValidator(
            new List<ICostModelPolicyRule>
            {
                new CostModelPolicyRequiredUnitRule("cost model policy needs this field"),
                new CostModelPolicyEndsAtRangeRule(86400, 86400),
                new CostModelPolicyAllowedCodeRule(new List<string> { "lenient", "primary", "primary" }),
                new CostModelPolicyUnitLengthRule(32),
            });
    }
}
