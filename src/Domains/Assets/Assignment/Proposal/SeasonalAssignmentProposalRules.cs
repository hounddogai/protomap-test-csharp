namespace Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>A rule a seasonal assignment proposal must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAssignmentProposalRule
{
    List<string> Check(SeasonalAssignmentProposal item);
}

public sealed record SeasonalAssignmentProposalRequiredCodeRule(string Message) : ISeasonalAssignmentProposalRule
{
    public List<string> Check(SeasonalAssignmentProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentProposalCountryLengthRule(int MaxLength) : ISeasonalAssignmentProposalRule
{
    public List<string> Check(SeasonalAssignmentProposal item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentProposalCodePrefixRule(string Prefix) : ISeasonalAssignmentProposalRule
{
    public List<string> Check(SeasonalAssignmentProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentProposalHoursRangeRule(double Min, double Max) : ISeasonalAssignmentProposalRule
{
    public List<string> Check(SeasonalAssignmentProposal item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal assignment proposal.</summary>
public sealed record SeasonalAssignmentProposalValidator(List<ISeasonalAssignmentProposalRule> Rules)
{
    public List<string> Validate(SeasonalAssignmentProposal item)
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

    public void RequireValid(SeasonalAssignmentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAssignmentProposal> items)
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

/// <summary>Functions of seasonal assignment proposal rules.</summary>
public static class SeasonalAssignmentProposalRules
{
    public static SeasonalAssignmentProposalValidator DefaultSeasonalAssignmentProposalValidator()
    {
        return new SeasonalAssignmentProposalValidator(
            new List<ISeasonalAssignmentProposalRule>
            {
                new SeasonalAssignmentProposalRequiredCodeRule("seasonal assignment proposal needs this field"),
                new SeasonalAssignmentProposalCountryLengthRule(86400),
                new SeasonalAssignmentProposalCodePrefixRule("Q"),
                new SeasonalAssignmentProposalHoursRangeRule(4.3, 819.2),
            });
    }
}
