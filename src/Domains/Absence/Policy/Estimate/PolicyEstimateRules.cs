namespace Acme.Workforce.Absence.Policy.Estimate;

/// <summary>A rule a policy estimate must pass. Check returns its problems, or none.</summary>
public interface IPolicyEstimateRule
{
    List<string> Check(PolicyEstimate item);
}

public sealed record PolicyEstimateTitleLengthRule(int MaxLength) : IPolicyEstimateRule
{
    public List<string> Check(PolicyEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEstimateContactEmailLengthRule(int MaxLength) : IPolicyEstimateRule
{
    public List<string> Check(PolicyEstimate item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEstimateDiscountRangeRule(double Min, double Max) : IPolicyEstimateRule
{
    public List<string> Check(PolicyEstimate item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEstimateJobCodeLengthRule(int MaxLength) : IPolicyEstimateRule
{
    public List<string> Check(PolicyEstimate item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEstimateBudgetRangeRule(double Min, double Max) : IPolicyEstimateRule
{
    public List<string> Check(PolicyEstimate item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy estimate.</summary>
public sealed record PolicyEstimateValidator(List<IPolicyEstimateRule> Rules)
{
    public List<string> Validate(PolicyEstimate item)
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

    public void RequireValid(PolicyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyEstimate> items)
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

/// <summary>Functions of policy estimate rules.</summary>
public static class PolicyEstimateRules
{
    public static PolicyEstimateValidator DefaultPolicyEstimateValidator()
    {
        return new PolicyEstimateValidator(
            new List<IPolicyEstimateRule>
            {
                new PolicyEstimateTitleLengthRule(32),
                new PolicyEstimateContactEmailLengthRule(16),
                new PolicyEstimateDiscountRangeRule(7.3, 4324.3),
                new PolicyEstimateJobCodeLengthRule(16),
                new PolicyEstimateBudgetRangeRule(2.4, 1364.5),
            });
    }
}
