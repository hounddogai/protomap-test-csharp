namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary policy ack draft must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyAckDraftRule
{
    List<string> Check(PrimaryPolicyAckDraft item);
}

public sealed record PrimaryPolicyAckDraftCeilingRangeRule(double Min, double Max) : IPrimaryPolicyAckDraftRule
{
    public List<string> Check(PrimaryPolicyAckDraft item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckDraftCodePrefixRule(string Prefix) : IPrimaryPolicyAckDraftRule
{
    public List<string> Check(PrimaryPolicyAckDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckDraftAllowedCodeRule(List<string> Allowed) : IPrimaryPolicyAckDraftRule
{
    public List<string> Check(PrimaryPolicyAckDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckDraftRequiredTitleRule(string Message) : IPrimaryPolicyAckDraftRule
{
    public List<string> Check(PrimaryPolicyAckDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckDraftCodeLengthRule(int MaxLength) : IPrimaryPolicyAckDraftRule
{
    public List<string> Check(PrimaryPolicyAckDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy ack draft.</summary>
public sealed record PrimaryPolicyAckDraftValidator(List<IPrimaryPolicyAckDraftRule> Rules)
{
    public List<string> Validate(PrimaryPolicyAckDraft item)
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

    public void RequireValid(PrimaryPolicyAckDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyAckDraft> items)
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

/// <summary>Functions of primary policy ack draft rules.</summary>
public static class PrimaryPolicyAckDraftRules
{
    public static PrimaryPolicyAckDraftValidator DefaultPrimaryPolicyAckDraftValidator()
    {
        return new PrimaryPolicyAckDraftValidator(
            new List<IPrimaryPolicyAckDraftRule>
            {
                new PrimaryPolicyAckDraftCeilingRangeRule(4.6, 3848.5),
                new PrimaryPolicyAckDraftCodePrefixRule("X"),
                new PrimaryPolicyAckDraftAllowedCodeRule(new List<string> { "tiered", "batched" }),
                new PrimaryPolicyAckDraftRequiredTitleRule("primary policy ack draft needs an entry"),
                new PrimaryPolicyAckDraftCodeLengthRule(16),
            });
    }
}
