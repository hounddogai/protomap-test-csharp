namespace Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>A rule a visitor proposal must pass. Check returns its problems, or none.</summary>
public interface IVisitorProposalRule
{
    List<string> Check(VisitorProposal item);
}

public sealed record VisitorProposalLabelLengthRule(int MaxLength) : IVisitorProposalRule
{
    public List<string> Check(VisitorProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorProposalRequiredDescriptionRule(string Message) : IVisitorProposalRule
{
    public List<string> Check(VisitorProposal item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorProposalDescriptionLengthRule(int MaxLength) : IVisitorProposalRule
{
    public List<string> Check(VisitorProposal item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorProposalCodeLengthRule(int MaxLength) : IVisitorProposalRule
{
    public List<string> Check(VisitorProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor proposal.</summary>
public sealed record VisitorProposalValidator(List<IVisitorProposalRule> Rules)
{
    public List<string> Validate(VisitorProposal item)
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

    public void RequireValid(VisitorProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorProposal> items)
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

/// <summary>Functions of visitor proposal rules.</summary>
public static class VisitorProposalRules
{
    public static VisitorProposalValidator DefaultVisitorProposalValidator()
    {
        return new VisitorProposalValidator(
            new List<IVisitorProposalRule>
            {
                new VisitorProposalLabelLengthRule(86400),
                new VisitorProposalRequiredDescriptionRule("visitor proposal needs an entry"),
                new VisitorProposalDescriptionLengthRule(16),
                new VisitorProposalCodeLengthRule(80),
            });
    }
}
