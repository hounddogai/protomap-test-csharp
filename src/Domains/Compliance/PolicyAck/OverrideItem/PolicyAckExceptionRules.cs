namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack exception must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckExceptionRule
{
    List<string> Check(PolicyAckException item);
}

public sealed record PolicyAckExceptionRequiredLabelRule(string Message) : IPolicyAckExceptionRule
{
    public List<string> Check(PolicyAckException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckExceptionAllowedExternalRefRule(List<string> Allowed) : IPolicyAckExceptionRule
{
    public List<string> Check(PolicyAckException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckExceptionCodeLengthRule(int MaxLength) : IPolicyAckExceptionRule
{
    public List<string> Check(PolicyAckException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack exception.</summary>
public sealed record PolicyAckExceptionValidator(List<IPolicyAckExceptionRule> Rules)
{
    public List<string> Validate(PolicyAckException item)
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

    public void RequireValid(PolicyAckException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckException> items)
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

/// <summary>Functions of policy ack exception rules.</summary>
public static class PolicyAckExceptionRules
{
    public static PolicyAckExceptionValidator DefaultPolicyAckExceptionValidator()
    {
        return new PolicyAckExceptionValidator(
            new List<IPolicyAckExceptionRule>
            {
                new PolicyAckExceptionRequiredLabelRule("policy ack exception needs this field"),
                new PolicyAckExceptionAllowedExternalRefRule(new List<string> { "rolling", "regional" }),
                new PolicyAckExceptionCodeLengthRule(3600),
            });
    }
}
