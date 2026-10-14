namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending cost model rule must pass. Check returns its problems, or none.</summary>
public interface IPendingCostModelRuleRule
{
    List<string> Check(PendingCostModelRule item);
}

public sealed record PendingCostModelRuleAllowedCodeRule(List<string> Allowed) : IPendingCostModelRuleRule
{
    public List<string> Check(PendingCostModelRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelRuleCodeLengthRule(int MaxLength) : IPendingCostModelRuleRule
{
    public List<string> Check(PendingCostModelRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelRuleRequiredCodeRule(string Message) : IPendingCostModelRuleRule
{
    public List<string> Check(PendingCostModelRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelRuleCodePrefixRule(string Prefix) : IPendingCostModelRuleRule
{
    public List<string> Check(PendingCostModelRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelRulePriorityRangeRule(int Min, int Max) : IPendingCostModelRuleRule
{
    public List<string> Check(PendingCostModelRule item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending cost model rule.</summary>
public sealed record PendingCostModelRuleValidator(List<IPendingCostModelRuleRule> Rules)
{
    public List<string> Validate(PendingCostModelRule item)
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

    public void RequireValid(PendingCostModelRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingCostModelRule> items)
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

/// <summary>Functions of pending cost model rule rules.</summary>
public static class PendingCostModelRuleRules
{
    public static PendingCostModelRuleValidator DefaultPendingCostModelRuleValidator()
    {
        return new PendingCostModelRuleValidator(
            new List<IPendingCostModelRuleRule>
            {
                new PendingCostModelRuleAllowedCodeRule(new List<string> { "pending" }),
                new PendingCostModelRuleCodeLengthRule(32),
                new PendingCostModelRuleRequiredCodeRule("pending cost model rule needs a value"),
                new PendingCostModelRuleCodePrefixRule("Q"),
                new PendingCostModelRulePriorityRangeRule(16, 80),
            });
    }
}
