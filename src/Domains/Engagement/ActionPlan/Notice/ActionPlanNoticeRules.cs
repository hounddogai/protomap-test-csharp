namespace Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>A rule a action plan notice must pass. Check returns its problems, or none.</summary>
public interface IActionPlanNoticeRule
{
    List<string> Check(ActionPlanNotice item);
}

public sealed record ActionPlanNoticeBadgeLengthRule(int MaxLength) : IActionPlanNoticeRule
{
    public List<string> Check(ActionPlanNotice item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanNoticeDurationMinutesRangeRule(int Min, int Max) : IActionPlanNoticeRule
{
    public List<string> Check(ActionPlanNotice item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanNoticeCodePrefixRule(string Prefix) : IActionPlanNoticeRule
{
    public List<string> Check(ActionPlanNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan notice.</summary>
public sealed record ActionPlanNoticeValidator(List<IActionPlanNoticeRule> Rules)
{
    public List<string> Validate(ActionPlanNotice item)
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

    public void RequireValid(ActionPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanNotice> items)
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

/// <summary>Functions of action plan notice rules.</summary>
public static class ActionPlanNoticeRules
{
    public static ActionPlanNoticeValidator DefaultActionPlanNoticeValidator()
    {
        return new ActionPlanNoticeValidator(
            new List<IActionPlanNoticeRule>
            {
                new ActionPlanNoticeBadgeLengthRule(16),
                new ActionPlanNoticeDurationMinutesRangeRule(16, 64),
                new ActionPlanNoticeCodePrefixRule("REF"),
            });
    }
}
