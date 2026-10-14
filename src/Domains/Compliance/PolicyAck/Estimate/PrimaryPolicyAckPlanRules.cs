namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>A rule a primary policy ack plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyAckPlanRule
{
    List<string> Check(PrimaryPolicyAckPlan item);
}

public sealed record PrimaryPolicyAckPlanRequiredBadgeRule(string Message) : IPrimaryPolicyAckPlanRule
{
    public List<string> Check(PrimaryPolicyAckPlan item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckPlanLabelLengthRule(int MaxLength) : IPrimaryPolicyAckPlanRule
{
    public List<string> Check(PrimaryPolicyAckPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckPlanRequiredCodeRule(string Message) : IPrimaryPolicyAckPlanRule
{
    public List<string> Check(PrimaryPolicyAckPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckPlanTeamLengthRule(int MaxLength) : IPrimaryPolicyAckPlanRule
{
    public List<string> Check(PrimaryPolicyAckPlan item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy ack plan.</summary>
public sealed record PrimaryPolicyAckPlanValidator(List<IPrimaryPolicyAckPlanRule> Rules)
{
    public List<string> Validate(PrimaryPolicyAckPlan item)
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

    public void RequireValid(PrimaryPolicyAckPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyAckPlan> items)
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

/// <summary>Functions of primary policy ack plan rules.</summary>
public static class PrimaryPolicyAckPlanRules
{
    public static PrimaryPolicyAckPlanValidator DefaultPrimaryPolicyAckPlanValidator()
    {
        return new PrimaryPolicyAckPlanValidator(
            new List<IPrimaryPolicyAckPlanRule>
            {
                new PrimaryPolicyAckPlanRequiredBadgeRule("primary policy ack plan needs an entry"),
                new PrimaryPolicyAckPlanLabelLengthRule(86400),
                new PrimaryPolicyAckPlanRequiredCodeRule("primary policy ack plan needs this field"),
                new PrimaryPolicyAckPlanTeamLengthRule(64),
            });
    }
}
