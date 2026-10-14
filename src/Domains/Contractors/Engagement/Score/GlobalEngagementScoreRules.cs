namespace Acme.Workforce.Contractors.Engagement.Score;

/// <summary>A rule a global engagement score must pass. Check returns its problems, or none.</summary>
public interface IGlobalEngagementScoreRule
{
    List<string> Check(GlobalEngagementScore item);
}

public sealed record GlobalEngagementScoreTitleLengthRule(int MaxLength) : IGlobalEngagementScoreRule
{
    public List<string> Check(GlobalEngagementScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementScoreCodeLengthRule(int MaxLength) : IGlobalEngagementScoreRule
{
    public List<string> Check(GlobalEngagementScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementScoreRequiredContactEmailRule(string Message) : IGlobalEngagementScoreRule
{
    public List<string> Check(GlobalEngagementScore item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global engagement score.</summary>
public sealed record GlobalEngagementScoreValidator(List<IGlobalEngagementScoreRule> Rules)
{
    public List<string> Validate(GlobalEngagementScore item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(GlobalEngagementScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEngagementScore> items)
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

/// <summary>Functions of global engagement score rules.</summary>
public static class GlobalEngagementScoreRules
{
    public static GlobalEngagementScoreValidator DefaultGlobalEngagementScoreValidator()
    {
        return new GlobalEngagementScoreValidator(
            new List<IGlobalEngagementScoreRule>
            {
                new GlobalEngagementScoreTitleLengthRule(120),
                new GlobalEngagementScoreCodeLengthRule(255),
                new GlobalEngagementScoreRequiredContactEmailRule("global engagement score needs this field"),
            });
    }
}
