namespace Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>A rule a legacy offboarding bundle must pass. Check returns its problems, or none.</summary>
public interface ILegacyOffboardingBundleRule
{
    List<string> Check(LegacyOffboardingBundle item);
}

public sealed record LegacyOffboardingBundleCostRangeRule(double Min, double Max) : ILegacyOffboardingBundleRule
{
    public List<string> Check(LegacyOffboardingBundle item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingBundleCodeLengthRule(int MaxLength) : ILegacyOffboardingBundleRule
{
    public List<string> Check(LegacyOffboardingBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingBundleRequiredCostCenterRule(string Message) : ILegacyOffboardingBundleRule
{
    public List<string> Check(LegacyOffboardingBundle item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy offboarding bundle.</summary>
public sealed record LegacyOffboardingBundleValidator(List<ILegacyOffboardingBundleRule> Rules)
{
    public List<string> Validate(LegacyOffboardingBundle item)
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

    public void RequireValid(LegacyOffboardingBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOffboardingBundle> items)
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

/// <summary>Functions of legacy offboarding bundle rules.</summary>
public static class LegacyOffboardingBundleRules
{
    public static LegacyOffboardingBundleValidator DefaultLegacyOffboardingBundleValidator()
    {
        return new LegacyOffboardingBundleValidator(
            new List<ILegacyOffboardingBundleRule>
            {
                new LegacyOffboardingBundleCostRangeRule(0.9, 1590.8),
                new LegacyOffboardingBundleCodeLengthRule(32),
                new LegacyOffboardingBundleRequiredCostCenterRule("legacy offboarding bundle needs an entry"),
            });
    }
}
