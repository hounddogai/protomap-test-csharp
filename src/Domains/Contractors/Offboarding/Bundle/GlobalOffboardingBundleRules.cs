namespace Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>A rule a global offboarding bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalOffboardingBundleRule
{
    List<string> Check(GlobalOffboardingBundle item);
}

public sealed record GlobalOffboardingBundleContactEmailLengthRule(int MaxLength) : IGlobalOffboardingBundleRule
{
    public List<string> Check(GlobalOffboardingBundle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingBundleCodeLengthRule(int MaxLength) : IGlobalOffboardingBundleRule
{
    public List<string> Check(GlobalOffboardingBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingBundleBudgetRangeRule(double Min, double Max) : IGlobalOffboardingBundleRule
{
    public List<string> Check(GlobalOffboardingBundle item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalOffboardingBundleRequiredContactEmailRule(string Message) : IGlobalOffboardingBundleRule
{
    public List<string> Check(GlobalOffboardingBundle item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingBundleSummaryLengthRule(int MaxLength) : IGlobalOffboardingBundleRule
{
    public List<string> Check(GlobalOffboardingBundle item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global offboarding bundle.</summary>
public sealed record GlobalOffboardingBundleValidator(List<IGlobalOffboardingBundleRule> Rules)
{
    public List<string> Validate(GlobalOffboardingBundle item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(GlobalOffboardingBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalOffboardingBundle> items)
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

/// <summary>Functions of global offboarding bundle rules.</summary>
public static class GlobalOffboardingBundleRules
{
    public static GlobalOffboardingBundleValidator DefaultGlobalOffboardingBundleValidator()
    {
        return new GlobalOffboardingBundleValidator(
            new List<IGlobalOffboardingBundleRule>
            {
                new GlobalOffboardingBundleContactEmailLengthRule(64),
                new GlobalOffboardingBundleCodeLengthRule(80),
                new GlobalOffboardingBundleBudgetRangeRule(0.3, 4579.2),
                new GlobalOffboardingBundleRequiredContactEmailRule("global offboarding bundle needs this field"),
                new GlobalOffboardingBundleSummaryLengthRule(1000),
            });
    }
}
