namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global access badge estimate must pass. Check returns its problems, or none.</summary>
public interface IGlobalAccessBadgeEstimateRule
{
    List<string> Check(GlobalAccessBadgeEstimate item);
}

public sealed record GlobalAccessBadgeEstimateCapacityRangeRule(int Min, int Max) : IGlobalAccessBadgeEstimateRule
{
    public List<string> Check(GlobalAccessBadgeEstimate item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAccessBadgeEstimateAllowedCodeRule(List<string> Allowed) : IGlobalAccessBadgeEstimateRule
{
    public List<string> Check(GlobalAccessBadgeEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeEstimateCodeLengthRule(int MaxLength) : IGlobalAccessBadgeEstimateRule
{
    public List<string> Check(GlobalAccessBadgeEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeEstimateNameLengthRule(int MaxLength) : IGlobalAccessBadgeEstimateRule
{
    public List<string> Check(GlobalAccessBadgeEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeEstimateMaximumRangeRule(double Min, double Max) : IGlobalAccessBadgeEstimateRule
{
    public List<string> Check(GlobalAccessBadgeEstimate item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global access badge estimate.</summary>
public sealed record GlobalAccessBadgeEstimateValidator(List<IGlobalAccessBadgeEstimateRule> Rules)
{
    public List<string> Validate(GlobalAccessBadgeEstimate item)
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

    public void RequireValid(GlobalAccessBadgeEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAccessBadgeEstimate> items)
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

/// <summary>Functions of global access badge estimate rules.</summary>
public static class GlobalAccessBadgeEstimateRules
{
    public static GlobalAccessBadgeEstimateValidator DefaultGlobalAccessBadgeEstimateValidator()
    {
        return new GlobalAccessBadgeEstimateValidator(
            new List<IGlobalAccessBadgeEstimateRule>
            {
                new GlobalAccessBadgeEstimateCapacityRangeRule(80, 1000),
                new GlobalAccessBadgeEstimateAllowedCodeRule(new List<string> { "regional" }),
                new GlobalAccessBadgeEstimateCodeLengthRule(32),
                new GlobalAccessBadgeEstimateNameLengthRule(32),
                new GlobalAccessBadgeEstimateMaximumRangeRule(2.6, 2261.2),
            });
    }
}
