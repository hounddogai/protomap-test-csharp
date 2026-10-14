namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan draft must pass. Check returns its problems, or none.</summary>
public interface IActionPlanDraftRule
{
    List<string> Check(ActionPlanDraft item);
}

public sealed record ActionPlanDraftPhoneLengthRule(int MaxLength) : IActionPlanDraftRule
{
    public List<string> Check(ActionPlanDraft item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDraftLimitRangeRule(int Min, int Max) : IActionPlanDraftRule
{
    public List<string> Check(ActionPlanDraft item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDraftRequiredCodeRule(string Message) : IActionPlanDraftRule
{
    public List<string> Check(ActionPlanDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDraftRequiredPhoneRule(string Message) : IActionPlanDraftRule
{
    public List<string> Check(ActionPlanDraft item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanDraftAllowedPhoneRule(List<string> Allowed) : IActionPlanDraftRule
{
    public List<string> Check(ActionPlanDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan draft.</summary>
public sealed record ActionPlanDraftValidator(List<IActionPlanDraftRule> Rules)
{
    public List<string> Validate(ActionPlanDraft item)
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

    public void RequireValid(ActionPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanDraft> items)
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

/// <summary>Functions of action plan draft rules.</summary>
public static class ActionPlanDraftRules
{
    public static ActionPlanDraftValidator DefaultActionPlanDraftValidator()
    {
        return new ActionPlanDraftValidator(
            new List<IActionPlanDraftRule>
            {
                new ActionPlanDraftPhoneLengthRule(120),
                new ActionPlanDraftLimitRangeRule(64, 3600),
                new ActionPlanDraftRequiredCodeRule("action plan draft needs this field"),
                new ActionPlanDraftRequiredPhoneRule("action plan draft needs this field"),
                new ActionPlanDraftAllowedPhoneRule(new List<string> { "regional" }),
            });
    }
}
