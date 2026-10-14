namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped feedback group must pass. Check returns its problems, or none.</summary>
public interface ICappedFeedbackGroupRule
{
    List<string> Check(CappedFeedbackGroup item);
}

public sealed record CappedFeedbackGroupManagerIdLengthRule(int MaxLength) : ICappedFeedbackGroupRule
{
    public List<string> Check(CappedFeedbackGroup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackGroupLabelLengthRule(int MaxLength) : ICappedFeedbackGroupRule
{
    public List<string> Check(CappedFeedbackGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackGroupAllowedDescriptionRule(List<string> Allowed) : ICappedFeedbackGroupRule
{
    public List<string> Check(CappedFeedbackGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackGroupRequiredManagerIdRule(string Message) : ICappedFeedbackGroupRule
{
    public List<string> Check(CappedFeedbackGroup item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackGroupRequiredDescriptionRule(string Message) : ICappedFeedbackGroupRule
{
    public List<string> Check(CappedFeedbackGroup item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped feedback group.</summary>
public sealed record CappedFeedbackGroupValidator(List<ICappedFeedbackGroupRule> Rules)
{
    public List<string> Validate(CappedFeedbackGroup item)
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

    public void RequireValid(CappedFeedbackGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedFeedbackGroup> items)
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

/// <summary>Functions of capped feedback group rules.</summary>
public static class CappedFeedbackGroupRules
{
    public static CappedFeedbackGroupValidator DefaultCappedFeedbackGroupValidator()
    {
        return new CappedFeedbackGroupValidator(
            new List<ICappedFeedbackGroupRule>
            {
                new CappedFeedbackGroupManagerIdLengthRule(64),
                new CappedFeedbackGroupLabelLengthRule(32),
                new CappedFeedbackGroupAllowedDescriptionRule(new List<string>()),
                new CappedFeedbackGroupRequiredManagerIdRule("capped feedback group needs a value"),
                new CappedFeedbackGroupRequiredDescriptionRule("capped feedback group needs a value"),
            });
    }
}
