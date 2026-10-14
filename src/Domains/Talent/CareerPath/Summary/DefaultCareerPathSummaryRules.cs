namespace Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>A rule a default career path summary must pass. Check returns its problems, or none.</summary>
public interface IDefaultCareerPathSummaryRule
{
    List<string> Check(DefaultCareerPathSummary item);
}

public sealed record DefaultCareerPathSummaryDescriptionLengthRule(int MaxLength) : IDefaultCareerPathSummaryRule
{
    public List<string> Check(DefaultCareerPathSummary item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathSummaryStartsAtRangeRule(int Min, int Max) : IDefaultCareerPathSummaryRule
{
    public List<string> Check(DefaultCareerPathSummary item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathSummaryEndsAtRangeRule(int Min, int Max) : IDefaultCareerPathSummaryRule
{
    public List<string> Check(DefaultCareerPathSummary item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default career path summary.</summary>
public sealed record DefaultCareerPathSummaryValidator(List<IDefaultCareerPathSummaryRule> Rules)
{
    public List<string> Validate(DefaultCareerPathSummary item)
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

    public void RequireValid(DefaultCareerPathSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultCareerPathSummary> items)
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

/// <summary>Functions of default career path summary rules.</summary>
public static class DefaultCareerPathSummaryRules
{
    public static DefaultCareerPathSummaryValidator DefaultDefaultCareerPathSummaryValidator()
    {
        return new DefaultCareerPathSummaryValidator(
            new List<IDefaultCareerPathSummaryRule>
            {
                new DefaultCareerPathSummaryDescriptionLengthRule(80),
                new DefaultCareerPathSummaryStartsAtRangeRule(64, 86400),
                new DefaultCareerPathSummaryEndsAtRangeRule(32, 255),
            });
    }
}
