namespace Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>A rule a regional warranty proposal must pass. Check returns its problems, or none.</summary>
public interface IRegionalWarrantyProposalRule
{
    List<string> Check(RegionalWarrantyProposal item);
}

public sealed record RegionalWarrantyProposalSourceLengthRule(int MaxLength) : IRegionalWarrantyProposalRule
{
    public List<string> Check(RegionalWarrantyProposal item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyProposalCodePrefixRule(string Prefix) : IRegionalWarrantyProposalRule
{
    public List<string> Check(RegionalWarrantyProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyProposalCreatedAtRangeRule(int Min, int Max) : IRegionalWarrantyProposalRule
{
    public List<string> Check(RegionalWarrantyProposal item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalWarrantyProposalNameLengthRule(int MaxLength) : IRegionalWarrantyProposalRule
{
    public List<string> Check(RegionalWarrantyProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional warranty proposal.</summary>
public sealed record RegionalWarrantyProposalValidator(List<IRegionalWarrantyProposalRule> Rules)
{
    public List<string> Validate(RegionalWarrantyProposal item)
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

    public void RequireValid(RegionalWarrantyProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalWarrantyProposal> items)
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

/// <summary>Functions of regional warranty proposal rules.</summary>
public static class RegionalWarrantyProposalRules
{
    public static RegionalWarrantyProposalValidator DefaultRegionalWarrantyProposalValidator()
    {
        return new RegionalWarrantyProposalValidator(
            new List<IRegionalWarrantyProposalRule>
            {
                new RegionalWarrantyProposalSourceLengthRule(32),
                new RegionalWarrantyProposalCodePrefixRule("ACM"),
                new RegionalWarrantyProposalCreatedAtRangeRule(16, 16),
                new RegionalWarrantyProposalNameLengthRule(80),
            });
    }
}
