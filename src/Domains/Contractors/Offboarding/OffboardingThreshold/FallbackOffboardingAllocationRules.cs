namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>A rule a fallback offboarding allocation must pass. Check returns its problems, or none.</summary>
public interface IFallbackOffboardingAllocationRule
{
    List<string> Check(FallbackOffboardingAllocation item);
}

public sealed record FallbackOffboardingAllocationRequiredExternalRefRule(string Message) : IFallbackOffboardingAllocationRule
{
    public List<string> Check(FallbackOffboardingAllocation item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackOffboardingAllocationRequiredNameRule(string Message) : IFallbackOffboardingAllocationRule
{
    public List<string> Check(FallbackOffboardingAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackOffboardingAllocationNameLengthRule(int MaxLength) : IFallbackOffboardingAllocationRule
{
    public List<string> Check(FallbackOffboardingAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback offboarding allocation.</summary>
public sealed record FallbackOffboardingAllocationValidator(List<IFallbackOffboardingAllocationRule> Rules)
{
    public List<string> Validate(FallbackOffboardingAllocation item)
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

    public void RequireValid(FallbackOffboardingAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackOffboardingAllocation> items)
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

/// <summary>Functions of fallback offboarding allocation rules.</summary>
public static class FallbackOffboardingAllocationRules
{
    public static FallbackOffboardingAllocationValidator DefaultFallbackOffboardingAllocationValidator()
    {
        return new FallbackOffboardingAllocationValidator(
            new List<IFallbackOffboardingAllocationRule>
            {
                new FallbackOffboardingAllocationRequiredExternalRefRule(
                    "fallback offboarding allocation needs a value"),
                new FallbackOffboardingAllocationRequiredNameRule("fallback offboarding allocation needs this field"),
                new FallbackOffboardingAllocationNameLengthRule(16),
            });
    }
}
