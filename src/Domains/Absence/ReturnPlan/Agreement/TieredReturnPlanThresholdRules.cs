namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered return plan threshold must pass. Check returns its problems, or none.</summary>
public interface ITieredReturnPlanThresholdRule
{
    List<string> Check(TieredReturnPlanThreshold item);
}

public sealed record TieredReturnPlanThresholdCodePrefixRule(string Prefix) : ITieredReturnPlanThresholdRule
{
    public List<string> Check(TieredReturnPlanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanThresholdAllowedCodeRule(List<string> Allowed) : ITieredReturnPlanThresholdRule
{
    public List<string> Check(TieredReturnPlanThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanThresholdRequiredLabelRule(string Message) : ITieredReturnPlanThresholdRule
{
    public List<string> Check(TieredReturnPlanThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered return plan threshold.</summary>
public sealed record TieredReturnPlanThresholdValidator(List<ITieredReturnPlanThresholdRule> Rules)
{
    public List<string> Validate(TieredReturnPlanThreshold item)
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

    public void RequireValid(TieredReturnPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredReturnPlanThreshold> items)
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

/// <summary>Functions of tiered return plan threshold rules.</summary>
public static class TieredReturnPlanThresholdRules
{
    public static TieredReturnPlanThresholdValidator DefaultTieredReturnPlanThresholdValidator()
    {
        return new TieredReturnPlanThresholdValidator(
            new List<ITieredReturnPlanThresholdRule>
            {
                new TieredReturnPlanThresholdCodePrefixRule("X"),
                new TieredReturnPlanThresholdAllowedCodeRule(new List<string> { "manual", "fallback" }),
                new TieredReturnPlanThresholdRequiredLabelRule("tiered return plan threshold needs an entry"),
            });
    }
}
