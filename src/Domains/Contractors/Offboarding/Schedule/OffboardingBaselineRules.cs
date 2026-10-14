namespace Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>A rule a offboarding baseline must pass. Check returns its problems, or none.</summary>
public interface IOffboardingBaselineRule
{
    List<string> Check(OffboardingBaseline item);
}

public sealed record OffboardingBaselineApprovedGuardRule(int Limit) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        if (item.Approved && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBaselineCodeLengthRule(int MaxLength) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBaselineRequiredLabelRule(string Message) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBaselineLimitRangeRule(int Min, int Max) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBaselineRequiredCodeRule(string Message) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBaselineTitleLengthRule(int MaxLength) : IOffboardingBaselineRule
{
    public List<string> Check(OffboardingBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding baseline.</summary>
public sealed record OffboardingBaselineValidator(List<IOffboardingBaselineRule> Rules)
{
    public List<string> Validate(OffboardingBaseline item)
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

    public void RequireValid(OffboardingBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingBaseline> items)
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

/// <summary>Functions of offboarding baseline rules.</summary>
public static class OffboardingBaselineRules
{
    public static OffboardingBaselineValidator DefaultOffboardingBaselineValidator()
    {
        return new OffboardingBaselineValidator(
            new List<IOffboardingBaselineRule>
            {
                new OffboardingBaselineApprovedGuardRule(255),
                new OffboardingBaselineCodeLengthRule(86400),
                new OffboardingBaselineRequiredLabelRule("offboarding baseline needs a value"),
                new OffboardingBaselineLimitRangeRule(64, 80),
                new OffboardingBaselineRequiredCodeRule("offboarding baseline needs an entry"),
                new OffboardingBaselineTitleLengthRule(255),
            });
    }
}
