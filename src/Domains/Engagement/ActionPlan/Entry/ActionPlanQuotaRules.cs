namespace Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>A rule a action plan quota must pass. Check returns its problems, or none.</summary>
public interface IActionPlanQuotaRule
{
    List<string> Check(ActionPlanQuota item);
}

public sealed record ActionPlanQuotaCodeLengthRule(int MaxLength) : IActionPlanQuotaRule
{
    public List<string> Check(ActionPlanQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanQuotaRequiredCodeRule(string Message) : IActionPlanQuotaRule
{
    public List<string> Check(ActionPlanQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanQuotaJobCodeLengthRule(int MaxLength) : IActionPlanQuotaRule
{
    public List<string> Check(ActionPlanQuota item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanQuotaCodePrefixRule(string Prefix) : IActionPlanQuotaRule
{
    public List<string> Check(ActionPlanQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanQuotaLimitRangeRule(int Min, int Max) : IActionPlanQuotaRule
{
    public List<string> Check(ActionPlanQuota item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a action plan quota.</summary>
public sealed record ActionPlanQuotaValidator(List<IActionPlanQuotaRule> Rules)
{
    public List<string> Validate(ActionPlanQuota item)
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

    public void RequireValid(ActionPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanQuota> items)
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

/// <summary>Functions of action plan quota rules.</summary>
public static class ActionPlanQuotaRules
{
    public static ActionPlanQuotaValidator DefaultActionPlanQuotaValidator()
    {
        return new ActionPlanQuotaValidator(
            new List<IActionPlanQuotaRule>
            {
                new ActionPlanQuotaCodeLengthRule(120),
                new ActionPlanQuotaRequiredCodeRule("action plan quota needs this field"),
                new ActionPlanQuotaJobCodeLengthRule(32),
                new ActionPlanQuotaCodePrefixRule("Q"),
                new ActionPlanQuotaLimitRangeRule(64, 120),
            });
    }
}
