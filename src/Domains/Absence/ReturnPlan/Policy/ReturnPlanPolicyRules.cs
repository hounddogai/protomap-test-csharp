namespace Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>A rule a return plan policy must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanPolicyRule
{
    List<string> Check(ReturnPlanPolicy item);
}

public sealed record ReturnPlanPolicyCodePrefixRule(string Prefix) : IReturnPlanPolicyRule
{
    public List<string> Check(ReturnPlanPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPolicyChannelLengthRule(int MaxLength) : IReturnPlanPolicyRule
{
    public List<string> Check(ReturnPlanPolicy item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPolicyRequiredLabelRule(string Message) : IReturnPlanPolicyRule
{
    public List<string> Check(ReturnPlanPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPolicyRequiredCodeRule(string Message) : IReturnPlanPolicyRule
{
    public List<string> Check(ReturnPlanPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan policy.</summary>
public sealed record ReturnPlanPolicyValidator(List<IReturnPlanPolicyRule> Rules)
{
    public List<string> Validate(ReturnPlanPolicy item)
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

    public void RequireValid(ReturnPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanPolicy> items)
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

/// <summary>Functions of return plan policy rules.</summary>
public static class ReturnPlanPolicyRules
{
    public static ReturnPlanPolicyValidator DefaultReturnPlanPolicyValidator()
    {
        return new ReturnPlanPolicyValidator(
            new List<IReturnPlanPolicyRule>
            {
                new ReturnPlanPolicyCodePrefixRule("Q"),
                new ReturnPlanPolicyChannelLengthRule(120),
                new ReturnPlanPolicyRequiredLabelRule("return plan policy needs an entry"),
                new ReturnPlanPolicyRequiredCodeRule("return plan policy needs a value"),
            });
    }
}
