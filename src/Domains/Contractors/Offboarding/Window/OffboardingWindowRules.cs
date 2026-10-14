namespace Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>A rule a offboarding window must pass. Check returns its problems, or none.</summary>
public interface IOffboardingWindowRule
{
    List<string> Check(OffboardingWindow item);
}

public sealed record OffboardingWindowCodeLengthRule(int MaxLength) : IOffboardingWindowRule
{
    public List<string> Check(OffboardingWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingWindowLocationLengthRule(int MaxLength) : IOffboardingWindowRule
{
    public List<string> Check(OffboardingWindow item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingWindowSalaryBandLengthRule(int MaxLength) : IOffboardingWindowRule
{
    public List<string> Check(OffboardingWindow item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding window.</summary>
public sealed record OffboardingWindowValidator(List<IOffboardingWindowRule> Rules)
{
    public List<string> Validate(OffboardingWindow item)
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

    public void RequireValid(OffboardingWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingWindow> items)
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

/// <summary>Functions of offboarding window rules.</summary>
public static class OffboardingWindowRules
{
    public static OffboardingWindowValidator DefaultOffboardingWindowValidator()
    {
        return new OffboardingWindowValidator(
            new List<IOffboardingWindowRule>
            {
                new OffboardingWindowCodeLengthRule(16),
                new OffboardingWindowLocationLengthRule(86400),
                new OffboardingWindowSalaryBandLengthRule(80),
            });
    }
}
