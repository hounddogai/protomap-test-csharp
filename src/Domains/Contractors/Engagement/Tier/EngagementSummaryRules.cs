namespace Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>A rule a engagement summary must pass. Check returns its problems, or none.</summary>
public interface IEngagementSummaryRule
{
    List<string> Check(EngagementSummary item);
}

public sealed record EngagementSummaryRequiredCodeRule(string Message) : IEngagementSummaryRule
{
    public List<string> Check(EngagementSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementSummaryRatingRangeRule(double Min, double Max) : IEngagementSummaryRule
{
    public List<string> Check(EngagementSummary item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementSummaryFteRangeRule(double Min, double Max) : IEngagementSummaryRule
{
    public List<string> Check(EngagementSummary item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementSummaryRequiredNameRule(string Message) : IEngagementSummaryRule
{
    public List<string> Check(EngagementSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement summary.</summary>
public sealed record EngagementSummaryValidator(List<IEngagementSummaryRule> Rules)
{
    public List<string> Validate(EngagementSummary item)
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

    public void RequireValid(EngagementSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementSummary> items)
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

/// <summary>Functions of engagement summary rules.</summary>
public static class EngagementSummaryRules
{
    public static EngagementSummaryValidator DefaultEngagementSummaryValidator()
    {
        return new EngagementSummaryValidator(
            new List<IEngagementSummaryRule>
            {
                new EngagementSummaryRequiredCodeRule("engagement summary needs a value"),
                new EngagementSummaryRatingRangeRule(5.7, 4323.6),
                new EngagementSummaryFteRangeRule(8.1, 4600.4),
                new EngagementSummaryRequiredNameRule("engagement summary needs a value"),
            });
    }
}
