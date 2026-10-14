namespace Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>A rule a equity grant proposal must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantProposalRule
{
    List<string> Check(EquityGrantProposal item);
}

public sealed record EquityGrantProposalRequiredExternalRefRule(string Message) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantProposalCodePrefixRule(string Prefix) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantProposalRequiredNotesRule(string Message) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantProposalExternalRefLengthRule(int MaxLength) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantProposalNotesLengthRule(int MaxLength) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantProposalRequiredNameRule(string Message) : IEquityGrantProposalRule
{
    public List<string> Check(EquityGrantProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant proposal.</summary>
public sealed record EquityGrantProposalValidator(List<IEquityGrantProposalRule> Rules)
{
    public List<string> Validate(EquityGrantProposal item)
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

    public void RequireValid(EquityGrantProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantProposal> items)
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

/// <summary>Functions of equity grant proposal rules.</summary>
public static class EquityGrantProposalRules
{
    public static EquityGrantProposalValidator DefaultEquityGrantProposalValidator()
    {
        return new EquityGrantProposalValidator(
            new List<IEquityGrantProposalRule>
            {
                new EquityGrantProposalRequiredExternalRefRule("equity grant proposal needs this field"),
                new EquityGrantProposalCodePrefixRule("Q"),
                new EquityGrantProposalRequiredNotesRule("equity grant proposal needs a value"),
                new EquityGrantProposalExternalRefLengthRule(1000),
                new EquityGrantProposalNotesLengthRule(3600),
                new EquityGrantProposalRequiredNameRule("equity grant proposal needs an entry"),
            });
    }
}
