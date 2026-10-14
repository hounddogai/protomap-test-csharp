namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>A rule a capped access badge template must pass. Check returns its problems, or none.</summary>
public interface ICappedAccessBadgeTemplateRule
{
    List<string> Check(CappedAccessBadgeTemplate item);
}

public sealed record CappedAccessBadgeTemplateRequiredSummaryRule(string Message) : ICappedAccessBadgeTemplateRule
{
    public List<string> Check(CappedAccessBadgeTemplate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeTemplateCodePrefixRule(string Prefix) : ICappedAccessBadgeTemplateRule
{
    public List<string> Check(CappedAccessBadgeTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeTemplateFloorValueRangeRule(double Min, double Max) : ICappedAccessBadgeTemplateRule
{
    public List<string> Check(CappedAccessBadgeTemplate item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAccessBadgeTemplateRequiredTitleRule(string Message) : ICappedAccessBadgeTemplateRule
{
    public List<string> Check(CappedAccessBadgeTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeTemplateSummaryLengthRule(int MaxLength) : ICappedAccessBadgeTemplateRule
{
    public List<string> Check(CappedAccessBadgeTemplate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped access badge template.</summary>
public sealed record CappedAccessBadgeTemplateValidator(List<ICappedAccessBadgeTemplateRule> Rules)
{
    public List<string> Validate(CappedAccessBadgeTemplate item)
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

    public void RequireValid(CappedAccessBadgeTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccessBadgeTemplate> items)
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

/// <summary>Functions of capped access badge template rules.</summary>
public static class CappedAccessBadgeTemplateRules
{
    public static CappedAccessBadgeTemplateValidator DefaultCappedAccessBadgeTemplateValidator()
    {
        return new CappedAccessBadgeTemplateValidator(
            new List<ICappedAccessBadgeTemplateRule>
            {
                new CappedAccessBadgeTemplateRequiredSummaryRule("capped access badge template needs a value"),
                new CappedAccessBadgeTemplateCodePrefixRule("Q"),
                new CappedAccessBadgeTemplateFloorValueRangeRule(8.2, 1131.7),
                new CappedAccessBadgeTemplateRequiredTitleRule("capped access badge template needs this field"),
                new CappedAccessBadgeTemplateSummaryLengthRule(3600),
            });
    }
}
