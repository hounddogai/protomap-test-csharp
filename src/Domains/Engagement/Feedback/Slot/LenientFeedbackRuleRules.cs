namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient feedback rule must pass. Check returns its problems, or none.</summary>
public interface ILenientFeedbackRuleRule
{
    List<string> Check(LenientFeedbackRule item);
}

public sealed record LenientFeedbackRuleTimezoneLengthRule(int MaxLength) : ILenientFeedbackRuleRule
{
    public List<string> Check(LenientFeedbackRule item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientFeedbackRuleRequiredTimezoneRule(string Message) : ILenientFeedbackRuleRule
{
    public List<string> Check(LenientFeedbackRule item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientFeedbackRuleRequiredChannelRule(string Message) : ILenientFeedbackRuleRule
{
    public List<string> Check(LenientFeedbackRule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientFeedbackRuleRequiredCodeRule(string Message) : ILenientFeedbackRuleRule
{
    public List<string> Check(LenientFeedbackRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientFeedbackRuleAllowedTimezoneRule(List<string> Allowed) : ILenientFeedbackRuleRule
{
    public List<string> Check(LenientFeedbackRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient feedback rule.</summary>
public sealed record LenientFeedbackRuleValidator(List<ILenientFeedbackRuleRule> Rules)
{
    public List<string> Validate(LenientFeedbackRule item)
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

    public void RequireValid(LenientFeedbackRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientFeedbackRule> items)
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

/// <summary>Functions of lenient feedback rule rules.</summary>
public static class LenientFeedbackRuleRules
{
    public static LenientFeedbackRuleValidator DefaultLenientFeedbackRuleValidator()
    {
        return new LenientFeedbackRuleValidator(
            new List<ILenientFeedbackRuleRule>
            {
                new LenientFeedbackRuleTimezoneLengthRule(3600),
                new LenientFeedbackRuleRequiredTimezoneRule("lenient feedback rule needs an entry"),
                new LenientFeedbackRuleRequiredChannelRule("lenient feedback rule needs a value"),
                new LenientFeedbackRuleRequiredCodeRule("lenient feedback rule needs this field"),
                new LenientFeedbackRuleAllowedTimezoneRule(new List<string> { "pending", "weighted", "weighted" }),
            });
    }
}
