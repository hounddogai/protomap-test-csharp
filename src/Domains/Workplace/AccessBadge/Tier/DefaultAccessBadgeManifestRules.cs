namespace Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>A rule a default access badge manifest must pass. Check returns its problems, or none.</summary>
public interface IDefaultAccessBadgeManifestRule
{
    List<string> Check(DefaultAccessBadgeManifest item);
}

public sealed record DefaultAccessBadgeManifestRequiredSegmentRule(string Message) : IDefaultAccessBadgeManifestRule
{
    public List<string> Check(DefaultAccessBadgeManifest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeManifestCodeLengthRule(int MaxLength) : IDefaultAccessBadgeManifestRule
{
    public List<string> Check(DefaultAccessBadgeManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeManifestSegmentLengthRule(int MaxLength) : IDefaultAccessBadgeManifestRule
{
    public List<string> Check(DefaultAccessBadgeManifest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeManifestRequiredCodeRule(string Message) : IDefaultAccessBadgeManifestRule
{
    public List<string> Check(DefaultAccessBadgeManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeManifestHoursRangeRule(double Min, double Max) : IDefaultAccessBadgeManifestRule
{
    public List<string> Check(DefaultAccessBadgeManifest item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default access badge manifest.</summary>
public sealed record DefaultAccessBadgeManifestValidator(List<IDefaultAccessBadgeManifestRule> Rules)
{
    public List<string> Validate(DefaultAccessBadgeManifest item)
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

    public void RequireValid(DefaultAccessBadgeManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAccessBadgeManifest> items)
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

/// <summary>Functions of default access badge manifest rules.</summary>
public static class DefaultAccessBadgeManifestRules
{
    public static DefaultAccessBadgeManifestValidator DefaultDefaultAccessBadgeManifestValidator()
    {
        return new DefaultAccessBadgeManifestValidator(
            new List<IDefaultAccessBadgeManifestRule>
            {
                new DefaultAccessBadgeManifestRequiredSegmentRule("default access badge manifest needs an entry"),
                new DefaultAccessBadgeManifestCodeLengthRule(3600),
                new DefaultAccessBadgeManifestSegmentLengthRule(16),
                new DefaultAccessBadgeManifestRequiredCodeRule("default access badge manifest needs this field"),
                new DefaultAccessBadgeManifestHoursRangeRule(5.2, 1254.6),
            });
    }
}
