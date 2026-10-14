namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan digest must pass. Check returns its problems, or none.</summary>
public interface IActionPlanDigestRule
{
    List<string> Check(ActionPlanDigest item);
}

public sealed record ActionPlanDigestUnitLengthRule(int MaxLength) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDigestVersionRangeRule(int Min, int Max) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanDigestCodeLengthRule(int MaxLength) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDigestAllowedChannelRule(List<string> Allowed) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDigestCodePrefixRule(string Prefix) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDigestRequiredTitleRule(string Message) : IActionPlanDigestRule
{
    public List<string> Check(ActionPlanDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan digest.</summary>
public sealed record ActionPlanDigestValidator(List<IActionPlanDigestRule> Rules)
{
    public List<string> Validate(ActionPlanDigest item)
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

    public void RequireValid(ActionPlanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanDigest> items)
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

/// <summary>Functions of action plan digest rules.</summary>
public static class ActionPlanDigestRules
{
    public static ActionPlanDigestValidator DefaultActionPlanDigestValidator()
    {
        return new ActionPlanDigestValidator(
            new List<IActionPlanDigestRule>
            {
                new ActionPlanDigestUnitLengthRule(16),
                new ActionPlanDigestVersionRangeRule(255, 1000),
                new ActionPlanDigestCodeLengthRule(3600),
                new ActionPlanDigestAllowedChannelRule(new List<string> { "rolling", "legacy" }),
                new ActionPlanDigestCodePrefixRule("OPS"),
                new ActionPlanDigestRequiredTitleRule("action plan digest needs a value"),
            });
    }
}
