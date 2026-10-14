namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>A rule a adaptive career path notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCareerPathNoticeRule
{
    List<string> Check(AdaptiveCareerPathNotice item);
}

public sealed record AdaptiveCareerPathNoticeRequiredSummaryRule(string Message) : IAdaptiveCareerPathNoticeRule
{
    public List<string> Check(AdaptiveCareerPathNotice item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathNoticeLabelLengthRule(int MaxLength) : IAdaptiveCareerPathNoticeRule
{
    public List<string> Check(AdaptiveCareerPathNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathNoticeRequiredJobCodeRule(string Message) : IAdaptiveCareerPathNoticeRule
{
    public List<string> Check(AdaptiveCareerPathNotice item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive career path notice.</summary>
public sealed record AdaptiveCareerPathNoticeValidator(List<IAdaptiveCareerPathNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveCareerPathNotice item)
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

    public void RequireValid(AdaptiveCareerPathNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCareerPathNotice> items)
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

/// <summary>Functions of adaptive career path notice rules.</summary>
public static class AdaptiveCareerPathNoticeRules
{
    public static AdaptiveCareerPathNoticeValidator DefaultAdaptiveCareerPathNoticeValidator()
    {
        return new AdaptiveCareerPathNoticeValidator(
            new List<IAdaptiveCareerPathNoticeRule>
            {
                new AdaptiveCareerPathNoticeRequiredSummaryRule("adaptive career path notice needs an entry"),
                new AdaptiveCareerPathNoticeLabelLengthRule(120),
                new AdaptiveCareerPathNoticeRequiredJobCodeRule("adaptive career path notice needs a value"),
            });
    }
}
