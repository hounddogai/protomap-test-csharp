namespace Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>A rule a capped engagement summary must pass. Check returns its problems, or none.</summary>
public interface ICappedEngagementSummaryRule
{
    List<string> Check(CappedEngagementSummary item);
}

public sealed record CappedEngagementSummaryRequiredGradeRule(string Message) : ICappedEngagementSummaryRule
{
    public List<string> Check(CappedEngagementSummary item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementSummaryRequiredLabelRule(string Message) : ICappedEngagementSummaryRule
{
    public List<string> Check(CappedEngagementSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementSummaryAttemptsRangeRule(int Min, int Max) : ICappedEngagementSummaryRule
{
    public List<string> Check(CappedEngagementSummary item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedEngagementSummaryRequiredCodeRule(string Message) : ICappedEngagementSummaryRule
{
    public List<string> Check(CappedEngagementSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementSummaryGradeLengthRule(int MaxLength) : ICappedEngagementSummaryRule
{
    public List<string> Check(CappedEngagementSummary item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped engagement summary.</summary>
public sealed record CappedEngagementSummaryValidator(List<ICappedEngagementSummaryRule> Rules)
{
    public List<string> Validate(CappedEngagementSummary item)
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

    public void RequireValid(CappedEngagementSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEngagementSummary> items)
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

/// <summary>Functions of capped engagement summary rules.</summary>
public static class CappedEngagementSummaryRules
{
    public static CappedEngagementSummaryValidator DefaultCappedEngagementSummaryValidator()
    {
        return new CappedEngagementSummaryValidator(
            new List<ICappedEngagementSummaryRule>
            {
                new CappedEngagementSummaryRequiredGradeRule("capped engagement summary needs an entry"),
                new CappedEngagementSummaryRequiredLabelRule("capped engagement summary needs this field"),
                new CappedEngagementSummaryAttemptsRangeRule(255, 1000),
                new CappedEngagementSummaryRequiredCodeRule("capped engagement summary needs this field"),
                new CappedEngagementSummaryGradeLengthRule(64),
            });
    }
}
