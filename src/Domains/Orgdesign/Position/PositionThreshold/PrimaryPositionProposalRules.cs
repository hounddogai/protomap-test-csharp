namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary position proposal must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPositionProposalRule
{
    List<string> Check(PrimaryPositionProposal item);
}

public sealed record PrimaryPositionProposalLabelLengthRule(int MaxLength) : IPrimaryPositionProposalRule
{
    public List<string> Check(PrimaryPositionProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProposalRequiredLabelRule(string Message) : IPrimaryPositionProposalRule
{
    public List<string> Check(PrimaryPositionProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProposalRequiredCodeRule(string Message) : IPrimaryPositionProposalRule
{
    public List<string> Check(PrimaryPositionProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProposalEndsAtRangeRule(int Min, int Max) : IPrimaryPositionProposalRule
{
    public List<string> Check(PrimaryPositionProposal item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProposalAllowedCodeRule(List<string> Allowed) : IPrimaryPositionProposalRule
{
    public List<string> Check(PrimaryPositionProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary position proposal.</summary>
public sealed record PrimaryPositionProposalValidator(List<IPrimaryPositionProposalRule> Rules)
{
    public List<string> Validate(PrimaryPositionProposal item)
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

    public void RequireValid(PrimaryPositionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPositionProposal> items)
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

/// <summary>Functions of primary position proposal rules.</summary>
public static class PrimaryPositionProposalRules
{
    public static PrimaryPositionProposalValidator DefaultPrimaryPositionProposalValidator()
    {
        return new PrimaryPositionProposalValidator(
            new List<IPrimaryPositionProposalRule>
            {
                new PrimaryPositionProposalLabelLengthRule(86400),
                new PrimaryPositionProposalRequiredLabelRule("primary position proposal needs an entry"),
                new PrimaryPositionProposalRequiredCodeRule("primary position proposal needs an entry"),
                new PrimaryPositionProposalEndsAtRangeRule(16, 120),
                new PrimaryPositionProposalAllowedCodeRule(new List<string> { "fallback" }),
            });
    }
}
