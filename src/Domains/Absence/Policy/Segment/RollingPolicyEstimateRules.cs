namespace Acme.Workforce.Absence.Policy.Segment;

/// <summary>A rule a rolling policy estimate must pass. Check returns its problems, or none.</summary>
public interface IRollingPolicyEstimateRule
{
    List<string> Check(RollingPolicyEstimate item);
}

public sealed record RollingPolicyEstimateCodeLengthRule(int MaxLength) : IRollingPolicyEstimateRule
{
    public List<string> Check(RollingPolicyEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyEstimateRequiredCurrencyRule(string Message) : IRollingPolicyEstimateRule
{
    public List<string> Check(RollingPolicyEstimate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyEstimateTitleLengthRule(int MaxLength) : IRollingPolicyEstimateRule
{
    public List<string> Check(RollingPolicyEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyEstimateCurrencyLengthRule(int MaxLength) : IRollingPolicyEstimateRule
{
    public List<string> Check(RollingPolicyEstimate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling policy estimate.</summary>
public sealed record RollingPolicyEstimateValidator(List<IRollingPolicyEstimateRule> Rules)
{
    public List<string> Validate(RollingPolicyEstimate item)
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

    public void RequireValid(RollingPolicyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPolicyEstimate> items)
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

/// <summary>Functions of rolling policy estimate rules.</summary>
public static class RollingPolicyEstimateRules
{
    public static RollingPolicyEstimateValidator DefaultRollingPolicyEstimateValidator()
    {
        return new RollingPolicyEstimateValidator(
            new List<IRollingPolicyEstimateRule>
            {
                new RollingPolicyEstimateCodeLengthRule(3600),
                new RollingPolicyEstimateRequiredCurrencyRule("rolling policy estimate needs this field"),
                new RollingPolicyEstimateTitleLengthRule(255),
                new RollingPolicyEstimateCurrencyLengthRule(32),
            });
    }
}
