namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>A rule a regional access badge score must pass. Check returns its problems, or none.</summary>
public interface IRegionalAccessBadgeScoreRule
{
    List<string> Check(RegionalAccessBadgeScore item);
}

public sealed record RegionalAccessBadgeScoreNameLengthRule(int MaxLength) : IRegionalAccessBadgeScoreRule
{
    public List<string> Check(RegionalAccessBadgeScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeScoreRequiredContactEmailRule(string Message) : IRegionalAccessBadgeScoreRule
{
    public List<string> Check(RegionalAccessBadgeScore item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeScoreScoreRangeRule(double Min, double Max) : IRegionalAccessBadgeScoreRule
{
    public List<string> Check(RegionalAccessBadgeScore item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional access badge score.</summary>
public sealed record RegionalAccessBadgeScoreValidator(List<IRegionalAccessBadgeScoreRule> Rules)
{
    public List<string> Validate(RegionalAccessBadgeScore item)
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

    public void RequireValid(RegionalAccessBadgeScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAccessBadgeScore> items)
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

/// <summary>Functions of regional access badge score rules.</summary>
public static class RegionalAccessBadgeScoreRules
{
    public static RegionalAccessBadgeScoreValidator DefaultRegionalAccessBadgeScoreValidator()
    {
        return new RegionalAccessBadgeScoreValidator(
            new List<IRegionalAccessBadgeScoreRule>
            {
                new RegionalAccessBadgeScoreNameLengthRule(86400),
                new RegionalAccessBadgeScoreRequiredContactEmailRule("regional access badge score needs this field"),
                new RegionalAccessBadgeScoreScoreRangeRule(6.6, 4196.6),
            });
    }
}
