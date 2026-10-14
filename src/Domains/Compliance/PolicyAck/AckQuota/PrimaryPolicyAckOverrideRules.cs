namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary policy ack override must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyAckOverrideRule
{
    List<string> Check(PrimaryPolicyAckOverride item);
}

public sealed record PrimaryPolicyAckOverrideAllowedNotesRule(List<string> Allowed) : IPrimaryPolicyAckOverrideRule
{
    public List<string> Check(PrimaryPolicyAckOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckOverrideRequiredCodeRule(string Message) : IPrimaryPolicyAckOverrideRule
{
    public List<string> Check(PrimaryPolicyAckOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckOverrideRequiredTitleRule(string Message) : IPrimaryPolicyAckOverrideRule
{
    public List<string> Check(PrimaryPolicyAckOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckOverrideTitleLengthRule(int MaxLength) : IPrimaryPolicyAckOverrideRule
{
    public List<string> Check(PrimaryPolicyAckOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy ack override.</summary>
public sealed record PrimaryPolicyAckOverrideValidator(List<IPrimaryPolicyAckOverrideRule> Rules)
{
    public List<string> Validate(PrimaryPolicyAckOverride item)
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

    public void RequireValid(PrimaryPolicyAckOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyAckOverride> items)
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

/// <summary>Functions of primary policy ack override rules.</summary>
public static class PrimaryPolicyAckOverrideRules
{
    public static PrimaryPolicyAckOverrideValidator DefaultPrimaryPolicyAckOverrideValidator()
    {
        return new PrimaryPolicyAckOverrideValidator(
            new List<IPrimaryPolicyAckOverrideRule>
            {
                new PrimaryPolicyAckOverrideAllowedNotesRule(new List<string> { "seasonal" }),
                new PrimaryPolicyAckOverrideRequiredCodeRule("primary policy ack override needs a value"),
                new PrimaryPolicyAckOverrideRequiredTitleRule("primary policy ack override needs a value"),
                new PrimaryPolicyAckOverrideTitleLengthRule(16),
            });
    }
}
