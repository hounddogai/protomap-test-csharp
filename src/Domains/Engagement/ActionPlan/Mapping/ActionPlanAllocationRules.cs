namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan allocation must pass. Check returns its problems, or none.</summary>
public interface IActionPlanAllocationRule
{
    List<string> Check(ActionPlanAllocation item);
}

public sealed record ActionPlanAllocationCodePrefixRule(string Prefix) : IActionPlanAllocationRule
{
    public List<string> Check(ActionPlanAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAllocationRequiredChannelRule(string Message) : IActionPlanAllocationRule
{
    public List<string> Check(ActionPlanAllocation item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAllocationChannelLengthRule(int MaxLength) : IActionPlanAllocationRule
{
    public List<string> Check(ActionPlanAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAllocationAllowedChannelRule(List<string> Allowed) : IActionPlanAllocationRule
{
    public List<string> Check(ActionPlanAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAllocationAmountRangeRule(double Min, double Max) : IActionPlanAllocationRule
{
    public List<string> Check(ActionPlanAllocation item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan allocation.</summary>
public sealed record ActionPlanAllocationValidator(List<IActionPlanAllocationRule> Rules)
{
    public List<string> Validate(ActionPlanAllocation item)
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

    public void RequireValid(ActionPlanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanAllocation> items)
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

/// <summary>Functions of action plan allocation rules.</summary>
public static class ActionPlanAllocationRules
{
    public static ActionPlanAllocationValidator DefaultActionPlanAllocationValidator()
    {
        return new ActionPlanAllocationValidator(
            new List<IActionPlanAllocationRule>
            {
                new ActionPlanAllocationCodePrefixRule("REF"),
                new ActionPlanAllocationRequiredChannelRule("action plan allocation needs an entry"),
                new ActionPlanAllocationChannelLengthRule(80),
                new ActionPlanAllocationAllowedChannelRule(new List<string> { "rolling", "adaptive", "manual" }),
                new ActionPlanAllocationAmountRangeRule(6.3, 3938.0),
            });
    }
}
