namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy milestone must pass. Check returns its problems, or none.</summary>
public interface IPolicyMilestoneRule
{
    List<string> Check(PolicyMilestone item);
}

public sealed record PolicyMilestoneRequiredContactEmailRule(string Message) : IPolicyMilestoneRule
{
    public List<string> Check(PolicyMilestone item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyMilestoneContactEmailLengthRule(int MaxLength) : IPolicyMilestoneRule
{
    public List<string> Check(PolicyMilestone item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyMilestoneAllowedContactEmailRule(List<string> Allowed) : IPolicyMilestoneRule
{
    public List<string> Check(PolicyMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy milestone.</summary>
public sealed record PolicyMilestoneValidator(List<IPolicyMilestoneRule> Rules)
{
    public List<string> Validate(PolicyMilestone item)
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

    public void RequireValid(PolicyMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyMilestone> items)
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

/// <summary>Functions of policy milestone rules.</summary>
public static class PolicyMilestoneRules
{
    public static PolicyMilestoneValidator DefaultPolicyMilestoneValidator()
    {
        return new PolicyMilestoneValidator(
            new List<IPolicyMilestoneRule>
            {
                new PolicyMilestoneRequiredContactEmailRule("policy milestone needs a value"),
                new PolicyMilestoneContactEmailLengthRule(64),
                new PolicyMilestoneAllowedContactEmailRule(new List<string>()),
            });
    }
}
