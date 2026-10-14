namespace Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>A rule a weighted offboarding threshold must pass. Check returns its problems, or none.</summary>
public interface IWeightedOffboardingThresholdRule
{
    List<string> Check(WeightedOffboardingThreshold item);
}

public sealed record WeightedOffboardingThresholdUnitLengthRule(int MaxLength) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOffboardingThresholdRequiredCodeRule(string Message) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedOffboardingThresholdRequiredContactEmailRule(string Message) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedOffboardingThresholdRequiredLabelRule(string Message) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedOffboardingThresholdCodeLengthRule(int MaxLength) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOffboardingThresholdVisibleGuardRule(int Limit) : IWeightedOffboardingThresholdRule
{
    public List<string> Check(WeightedOffboardingThreshold item)
    {
        if (item.Visible && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted offboarding threshold.</summary>
public sealed record WeightedOffboardingThresholdValidator(List<IWeightedOffboardingThresholdRule> Rules)
{
    public List<string> Validate(WeightedOffboardingThreshold item)
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

    public void RequireValid(WeightedOffboardingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedOffboardingThreshold> items)
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

/// <summary>Functions of weighted offboarding threshold rules.</summary>
public static class WeightedOffboardingThresholdRules
{
    public static WeightedOffboardingThresholdValidator DefaultWeightedOffboardingThresholdValidator()
    {
        return new WeightedOffboardingThresholdValidator(
            new List<IWeightedOffboardingThresholdRule>
            {
                new WeightedOffboardingThresholdUnitLengthRule(3600),
                new WeightedOffboardingThresholdRequiredCodeRule("weighted offboarding threshold needs this field"),
                new WeightedOffboardingThresholdRequiredContactEmailRule(
                    "weighted offboarding threshold needs this field"),
                new WeightedOffboardingThresholdRequiredLabelRule("weighted offboarding threshold needs an entry"),
                new WeightedOffboardingThresholdCodeLengthRule(64),
                new WeightedOffboardingThresholdVisibleGuardRule(16),
            });
    }
}
