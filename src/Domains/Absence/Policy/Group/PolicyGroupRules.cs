namespace Acme.Workforce.Absence.Policy.Group;

/// <summary>A rule a policy group must pass. Check returns its problems, or none.</summary>
public interface IPolicyGroupRule
{
    List<string> Check(PolicyGroup item);
}

public sealed record PolicyGroupRequiredNameRule(string Message) : IPolicyGroupRule
{
    public List<string> Check(PolicyGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyGroupReviewerLengthRule(int MaxLength) : IPolicyGroupRule
{
    public List<string> Check(PolicyGroup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyGroupScoreRangeRule(double Min, double Max) : IPolicyGroupRule
{
    public List<string> Check(PolicyGroup item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy group.</summary>
public sealed record PolicyGroupValidator(List<IPolicyGroupRule> Rules)
{
    public List<string> Validate(PolicyGroup item)
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

    public void RequireValid(PolicyGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyGroup> items)
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

/// <summary>Functions of policy group rules.</summary>
public static class PolicyGroupRules
{
    public static PolicyGroupValidator DefaultPolicyGroupValidator()
    {
        return new PolicyGroupValidator(
            new List<IPolicyGroupRule>
            {
                new PolicyGroupRequiredNameRule("policy group needs this field"),
                new PolicyGroupReviewerLengthRule(3600),
                new PolicyGroupScoreRangeRule(0.8, 3045.4),
            });
    }
}
