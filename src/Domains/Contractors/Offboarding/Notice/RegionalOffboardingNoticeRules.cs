namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional offboarding notice must pass. Check returns its problems, or none.</summary>
public interface IRegionalOffboardingNoticeRule
{
    List<string> Check(RegionalOffboardingNotice item);
}

public sealed record RegionalOffboardingNoticeDescriptionLengthRule(int MaxLength) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingNoticeRequiredDescriptionRule(string Message) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingNoticeUnitLengthRule(int MaxLength) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingNoticeRequiredLabelRule(string Message) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingNoticeCostRangeRule(double Min, double Max) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalOffboardingNoticeAllowedUnitRule(List<string> Allowed) : IRegionalOffboardingNoticeRule
{
    public List<string> Check(RegionalOffboardingNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional offboarding notice.</summary>
public sealed record RegionalOffboardingNoticeValidator(List<IRegionalOffboardingNoticeRule> Rules)
{
    public List<string> Validate(RegionalOffboardingNotice item)
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

    public void RequireValid(RegionalOffboardingNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalOffboardingNotice> items)
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

/// <summary>Functions of regional offboarding notice rules.</summary>
public static class RegionalOffboardingNoticeRules
{
    public static RegionalOffboardingNoticeValidator DefaultRegionalOffboardingNoticeValidator()
    {
        return new RegionalOffboardingNoticeValidator(
            new List<IRegionalOffboardingNoticeRule>
            {
                new RegionalOffboardingNoticeDescriptionLengthRule(3600),
                new RegionalOffboardingNoticeRequiredDescriptionRule("regional offboarding notice needs a value"),
                new RegionalOffboardingNoticeUnitLengthRule(80),
                new RegionalOffboardingNoticeRequiredLabelRule("regional offboarding notice needs a value"),
                new RegionalOffboardingNoticeCostRangeRule(8.6, 4584.4),
                new RegionalOffboardingNoticeAllowedUnitRule(new List<string> { "capped", "regional", "lenient" }),
            });
    }
}
