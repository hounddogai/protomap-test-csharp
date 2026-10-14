namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>A rule a adaptive leave request policy must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLeaveRequestPolicyRule
{
    List<string> Check(AdaptiveLeaveRequestPolicy item);
}

public sealed record AdaptiveLeaveRequestPolicyMinimumRangeRule(double Min, double Max) : IAdaptiveLeaveRequestPolicyRule
{
    public List<string> Check(AdaptiveLeaveRequestPolicy item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestPolicyRequiredNameRule(string Message) : IAdaptiveLeaveRequestPolicyRule
{
    public List<string> Check(AdaptiveLeaveRequestPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestPolicyCategoryLengthRule(int MaxLength) : IAdaptiveLeaveRequestPolicyRule
{
    public List<string> Check(AdaptiveLeaveRequestPolicy item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestPolicyJobCodeLengthRule(int MaxLength) : IAdaptiveLeaveRequestPolicyRule
{
    public List<string> Check(AdaptiveLeaveRequestPolicy item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestPolicyRetriesRangeRule(int Min, int Max) : IAdaptiveLeaveRequestPolicyRule
{
    public List<string> Check(AdaptiveLeaveRequestPolicy item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive leave request policy.</summary>
public sealed record AdaptiveLeaveRequestPolicyValidator(List<IAdaptiveLeaveRequestPolicyRule> Rules)
{
    public List<string> Validate(AdaptiveLeaveRequestPolicy item)
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

    public void RequireValid(AdaptiveLeaveRequestPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLeaveRequestPolicy> items)
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

/// <summary>Functions of adaptive leave request policy rules.</summary>
public static class AdaptiveLeaveRequestPolicyRules
{
    public static AdaptiveLeaveRequestPolicyValidator DefaultAdaptiveLeaveRequestPolicyValidator()
    {
        return new AdaptiveLeaveRequestPolicyValidator(
            new List<IAdaptiveLeaveRequestPolicyRule>
            {
                new AdaptiveLeaveRequestPolicyMinimumRangeRule(4.0, 4190.3),
                new AdaptiveLeaveRequestPolicyRequiredNameRule("adaptive leave request policy needs a value"),
                new AdaptiveLeaveRequestPolicyCategoryLengthRule(80),
                new AdaptiveLeaveRequestPolicyJobCodeLengthRule(16),
                new AdaptiveLeaveRequestPolicyRetriesRangeRule(80, 1000),
            });
    }
}
