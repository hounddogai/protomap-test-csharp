namespace Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>A rule a adaptive skill profile summary must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveSkillProfileSummaryRule
{
    List<string> Check(AdaptiveSkillProfileSummary item);
}

public sealed record AdaptiveSkillProfileSummaryCodeLengthRule(int MaxLength) : IAdaptiveSkillProfileSummaryRule
{
    public List<string> Check(AdaptiveSkillProfileSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSummaryRequiredCodeRule(string Message) : IAdaptiveSkillProfileSummaryRule
{
    public List<string> Check(AdaptiveSkillProfileSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSummarySummaryLengthRule(int MaxLength) : IAdaptiveSkillProfileSummaryRule
{
    public List<string> Check(AdaptiveSkillProfileSummary item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSummaryArchivedGuardRule(int Limit) : IAdaptiveSkillProfileSummaryRule
{
    public List<string> Check(AdaptiveSkillProfileSummary item)
    {
        if (item.Archived && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive skill profile summary.</summary>
public sealed record AdaptiveSkillProfileSummaryValidator(List<IAdaptiveSkillProfileSummaryRule> Rules)
{
    public List<string> Validate(AdaptiveSkillProfileSummary item)
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

    public void RequireValid(AdaptiveSkillProfileSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveSkillProfileSummary> items)
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

/// <summary>Functions of adaptive skill profile summary rules.</summary>
public static class AdaptiveSkillProfileSummaryRules
{
    public static AdaptiveSkillProfileSummaryValidator DefaultAdaptiveSkillProfileSummaryValidator()
    {
        return new AdaptiveSkillProfileSummaryValidator(
            new List<IAdaptiveSkillProfileSummaryRule>
            {
                new AdaptiveSkillProfileSummaryCodeLengthRule(86400),
                new AdaptiveSkillProfileSummaryRequiredCodeRule("adaptive skill profile summary needs this field"),
                new AdaptiveSkillProfileSummarySummaryLengthRule(120),
                new AdaptiveSkillProfileSummaryArchivedGuardRule(64),
            });
    }
}
