namespace Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>A rule a regional career path summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalCareerPathSummaryRule
{
    List<string> Check(RegionalCareerPathSummary item);
}

public sealed record RegionalCareerPathSummaryCodePrefixRule(string Prefix) : IRegionalCareerPathSummaryRule
{
    public List<string> Check(RegionalCareerPathSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathSummaryCodeLengthRule(int MaxLength) : IRegionalCareerPathSummaryRule
{
    public List<string> Check(RegionalCareerPathSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathSummaryCostRangeRule(double Min, double Max) : IRegionalCareerPathSummaryRule
{
    public List<string> Check(RegionalCareerPathSummary item)
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

/// <summary>Runs every rule of a regional career path summary.</summary>
public sealed record RegionalCareerPathSummaryValidator(List<IRegionalCareerPathSummaryRule> Rules)
{
    public List<string> Validate(RegionalCareerPathSummary item)
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

    public void RequireValid(RegionalCareerPathSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalCareerPathSummary> items)
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

/// <summary>Functions of regional career path summary rules.</summary>
public static class RegionalCareerPathSummaryRules
{
    public static RegionalCareerPathSummaryValidator DefaultRegionalCareerPathSummaryValidator()
    {
        return new RegionalCareerPathSummaryValidator(
            new List<IRegionalCareerPathSummaryRule>
            {
                new RegionalCareerPathSummaryCodePrefixRule("OPS"),
                new RegionalCareerPathSummaryCodeLengthRule(80),
                new RegionalCareerPathSummaryCostRangeRule(9.3, 980.8),
            });
    }
}
