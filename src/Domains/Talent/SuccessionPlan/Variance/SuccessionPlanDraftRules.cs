namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>A rule a succession plan draft must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanDraftRule
{
    List<string> Check(SuccessionPlanDraft item);
}

public sealed record SuccessionPlanDraftReviewerLengthRule(int MaxLength) : ISuccessionPlanDraftRule
{
    public List<string> Check(SuccessionPlanDraft item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanDraftRequiredTitleRule(string Message) : ISuccessionPlanDraftRule
{
    public List<string> Check(SuccessionPlanDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanDraftPhoneLengthRule(int MaxLength) : ISuccessionPlanDraftRule
{
    public List<string> Check(SuccessionPlanDraft item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanDraftRequiredPhoneRule(string Message) : ISuccessionPlanDraftRule
{
    public List<string> Check(SuccessionPlanDraft item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanDraftRequiredReviewerRule(string Message) : ISuccessionPlanDraftRule
{
    public List<string> Check(SuccessionPlanDraft item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan draft.</summary>
public sealed record SuccessionPlanDraftValidator(List<ISuccessionPlanDraftRule> Rules)
{
    public List<string> Validate(SuccessionPlanDraft item)
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

    public void RequireValid(SuccessionPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanDraft> items)
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

/// <summary>Functions of succession plan draft rules.</summary>
public static class SuccessionPlanDraftRules
{
    public static SuccessionPlanDraftValidator DefaultSuccessionPlanDraftValidator()
    {
        return new SuccessionPlanDraftValidator(
            new List<ISuccessionPlanDraftRule>
            {
                new SuccessionPlanDraftReviewerLengthRule(3600),
                new SuccessionPlanDraftRequiredTitleRule("succession plan draft needs a value"),
                new SuccessionPlanDraftPhoneLengthRule(120),
                new SuccessionPlanDraftRequiredPhoneRule("succession plan draft needs this field"),
                new SuccessionPlanDraftRequiredReviewerRule("succession plan draft needs this field"),
            });
    }
}
