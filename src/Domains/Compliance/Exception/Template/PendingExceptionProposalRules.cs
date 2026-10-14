namespace Acme.Workforce.Compliance.Exception.Template;

/// <summary>A rule a pending exception proposal must pass. Check returns its problems, or none.</summary>
public interface IPendingExceptionProposalRule
{
    List<string> Check(PendingExceptionProposal item);
}

public sealed record PendingExceptionProposalAttemptsRangeRule(int Min, int Max) : IPendingExceptionProposalRule
{
    public List<string> Check(PendingExceptionProposal item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionProposalCodePrefixRule(string Prefix) : IPendingExceptionProposalRule
{
    public List<string> Check(PendingExceptionProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionProposalRequiredTitleRule(string Message) : IPendingExceptionProposalRule
{
    public List<string> Check(PendingExceptionProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending exception proposal.</summary>
public sealed record PendingExceptionProposalValidator(List<IPendingExceptionProposalRule> Rules)
{
    public List<string> Validate(PendingExceptionProposal item)
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

    public void RequireValid(PendingExceptionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingExceptionProposal> items)
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

/// <summary>Functions of pending exception proposal rules.</summary>
public static class PendingExceptionProposalRules
{
    public static PendingExceptionProposalValidator DefaultPendingExceptionProposalValidator()
    {
        return new PendingExceptionProposalValidator(
            new List<IPendingExceptionProposalRule>
            {
                new PendingExceptionProposalAttemptsRangeRule(64, 255),
                new PendingExceptionProposalCodePrefixRule("X"),
                new PendingExceptionProposalRequiredTitleRule("pending exception proposal needs this field"),
            });
    }
}
