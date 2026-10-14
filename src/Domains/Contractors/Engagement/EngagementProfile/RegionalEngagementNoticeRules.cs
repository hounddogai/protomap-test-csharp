namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

/// <summary>A rule a regional engagement notice must pass. Check returns its problems, or none.</summary>
public interface IRegionalEngagementNoticeRule
{
    List<string> Check(RegionalEngagementNotice item);
}

public sealed record RegionalEngagementNoticeRequiredCodeRule(string Message) : IRegionalEngagementNoticeRule
{
    public List<string> Check(RegionalEngagementNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementNoticeExpiresAtRangeRule(int Min, int Max) : IRegionalEngagementNoticeRule
{
    public List<string> Check(RegionalEngagementNotice item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalEngagementNoticeTitleLengthRule(int MaxLength) : IRegionalEngagementNoticeRule
{
    public List<string> Check(RegionalEngagementNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementNoticeDiscountRangeRule(double Min, double Max) : IRegionalEngagementNoticeRule
{
    public List<string> Check(RegionalEngagementNotice item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional engagement notice.</summary>
public sealed record RegionalEngagementNoticeValidator(List<IRegionalEngagementNoticeRule> Rules)
{
    public List<string> Validate(RegionalEngagementNotice item)
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

    public void RequireValid(RegionalEngagementNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalEngagementNotice> items)
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

/// <summary>Functions of regional engagement notice rules.</summary>
public static class RegionalEngagementNoticeRules
{
    public static RegionalEngagementNoticeValidator DefaultRegionalEngagementNoticeValidator()
    {
        return new RegionalEngagementNoticeValidator(
            new List<IRegionalEngagementNoticeRule>
            {
                new RegionalEngagementNoticeRequiredCodeRule("regional engagement notice needs this field"),
                new RegionalEngagementNoticeExpiresAtRangeRule(80, 1000),
                new RegionalEngagementNoticeTitleLengthRule(3600),
                new RegionalEngagementNoticeDiscountRangeRule(5.1, 2851.5),
            });
    }
}
