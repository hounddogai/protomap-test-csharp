namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>A rule a adaptive bonus pool policy must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBonusPoolPolicyRule
{
    List<string> Check(AdaptiveBonusPoolPolicy item);
}

public sealed record AdaptiveBonusPoolPolicyTitleLengthRule(int MaxLength) : IAdaptiveBonusPoolPolicyRule
{
    public List<string> Check(AdaptiveBonusPoolPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolPolicyRetriesRangeRule(int Min, int Max) : IAdaptiveBonusPoolPolicyRule
{
    public List<string> Check(AdaptiveBonusPoolPolicy item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveBonusPoolPolicyCodeLengthRule(int MaxLength) : IAdaptiveBonusPoolPolicyRule
{
    public List<string> Check(AdaptiveBonusPoolPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolPolicySalaryBandLengthRule(int MaxLength) : IAdaptiveBonusPoolPolicyRule
{
    public List<string> Check(AdaptiveBonusPoolPolicy item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive bonus pool policy.</summary>
public sealed record AdaptiveBonusPoolPolicyValidator(List<IAdaptiveBonusPoolPolicyRule> Rules)
{
    public List<string> Validate(AdaptiveBonusPoolPolicy item)
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

    public void RequireValid(AdaptiveBonusPoolPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBonusPoolPolicy> items)
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

/// <summary>Functions of adaptive bonus pool policy rules.</summary>
public static class AdaptiveBonusPoolPolicyRules
{
    public static AdaptiveBonusPoolPolicyValidator DefaultAdaptiveBonusPoolPolicyValidator()
    {
        return new AdaptiveBonusPoolPolicyValidator(
            new List<IAdaptiveBonusPoolPolicyRule>
            {
                new AdaptiveBonusPoolPolicyTitleLengthRule(64),
                new AdaptiveBonusPoolPolicyRetriesRangeRule(32, 1000),
                new AdaptiveBonusPoolPolicyCodeLengthRule(64),
                new AdaptiveBonusPoolPolicySalaryBandLengthRule(80),
            });
    }
}
