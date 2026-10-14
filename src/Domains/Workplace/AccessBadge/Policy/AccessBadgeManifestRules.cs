namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>A rule a access badge manifest must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeManifestRule
{
    List<string> Check(AccessBadgeManifest item);
}

public sealed record AccessBadgeManifestScoreRangeRule(double Min, double Max) : IAccessBadgeManifestRule
{
    public List<string> Check(AccessBadgeManifest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeManifestRequiredSalaryBandRule(string Message) : IAccessBadgeManifestRule
{
    public List<string> Check(AccessBadgeManifest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeManifestLabelLengthRule(int MaxLength) : IAccessBadgeManifestRule
{
    public List<string> Check(AccessBadgeManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge manifest.</summary>
public sealed record AccessBadgeManifestValidator(List<IAccessBadgeManifestRule> Rules)
{
    public List<string> Validate(AccessBadgeManifest item)
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

    public void RequireValid(AccessBadgeManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeManifest> items)
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

/// <summary>Functions of access badge manifest rules.</summary>
public static class AccessBadgeManifestRules
{
    public static AccessBadgeManifestValidator DefaultAccessBadgeManifestValidator()
    {
        return new AccessBadgeManifestValidator(
            new List<IAccessBadgeManifestRule>
            {
                new AccessBadgeManifestScoreRangeRule(7.9, 4916.3),
                new AccessBadgeManifestRequiredSalaryBandRule("access badge manifest needs this field"),
                new AccessBadgeManifestLabelLengthRule(1000),
            });
    }
}
