namespace Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>A rule a offboarding rollup must pass. Check returns its problems, or none.</summary>
public interface IOffboardingRollupRule
{
    List<string> Check(OffboardingRollup item);
}

public sealed record OffboardingRollupBudgetRangeRule(double Min, double Max) : IOffboardingRollupRule
{
    public List<string> Check(OffboardingRollup item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRollupCreatedAtRangeRule(int Min, int Max) : IOffboardingRollupRule
{
    public List<string> Check(OffboardingRollup item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRollupRequiredRegionRule(string Message) : IOffboardingRollupRule
{
    public List<string> Check(OffboardingRollup item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding rollup.</summary>
public sealed record OffboardingRollupValidator(List<IOffboardingRollupRule> Rules)
{
    public List<string> Validate(OffboardingRollup item)
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

    public void RequireValid(OffboardingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingRollup> items)
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

/// <summary>Functions of offboarding rollup rules.</summary>
public static class OffboardingRollupRules
{
    public static OffboardingRollupValidator DefaultOffboardingRollupValidator()
    {
        return new OffboardingRollupValidator(
            new List<IOffboardingRollupRule>
            {
                new OffboardingRollupBudgetRangeRule(1.6, 4067.7),
                new OffboardingRollupCreatedAtRangeRule(80, 86400),
                new OffboardingRollupRequiredRegionRule("offboarding rollup needs an entry"),
            });
    }
}
