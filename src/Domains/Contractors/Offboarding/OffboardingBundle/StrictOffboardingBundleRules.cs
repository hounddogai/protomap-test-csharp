namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict offboarding bundle must pass. Check returns its problems, or none.</summary>
public interface IStrictOffboardingBundleRule
{
    List<string> Check(StrictOffboardingBundle item);
}

public sealed record StrictOffboardingBundleRequiredDepartmentRule(string Message) : IStrictOffboardingBundleRule
{
    public List<string> Check(StrictOffboardingBundle item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingBundleCodeLengthRule(int MaxLength) : IStrictOffboardingBundleRule
{
    public List<string> Check(StrictOffboardingBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingBundleCostRangeRule(double Min, double Max) : IStrictOffboardingBundleRule
{
    public List<string> Check(StrictOffboardingBundle item)
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

public sealed record StrictOffboardingBundleRequiredCodeRule(string Message) : IStrictOffboardingBundleRule
{
    public List<string> Check(StrictOffboardingBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingBundleAllowedDepartmentRule(List<string> Allowed) : IStrictOffboardingBundleRule
{
    public List<string> Check(StrictOffboardingBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict offboarding bundle.</summary>
public sealed record StrictOffboardingBundleValidator(List<IStrictOffboardingBundleRule> Rules)
{
    public List<string> Validate(StrictOffboardingBundle item)
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

    public void RequireValid(StrictOffboardingBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOffboardingBundle> items)
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

/// <summary>Functions of strict offboarding bundle rules.</summary>
public static class StrictOffboardingBundleRules
{
    public static StrictOffboardingBundleValidator DefaultStrictOffboardingBundleValidator()
    {
        return new StrictOffboardingBundleValidator(
            new List<IStrictOffboardingBundleRule>
            {
                new StrictOffboardingBundleRequiredDepartmentRule("strict offboarding bundle needs an entry"),
                new StrictOffboardingBundleCodeLengthRule(80),
                new StrictOffboardingBundleCostRangeRule(5.1, 3706.0),
                new StrictOffboardingBundleRequiredCodeRule("strict offboarding bundle needs an entry"),
                new StrictOffboardingBundleAllowedDepartmentRule(new List<string> { "lenient", "regional" }),
            });
    }
}
