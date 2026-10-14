namespace Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>A rule a seasonal adjustment proposal must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAdjustmentProposalRule
{
    List<string> Check(SeasonalAdjustmentProposal item);
}

public sealed record SeasonalAdjustmentProposalRequiredCodeRule(string Message) : ISeasonalAdjustmentProposalRule
{
    public List<string> Check(SeasonalAdjustmentProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentProposalRequiredLabelRule(string Message) : ISeasonalAdjustmentProposalRule
{
    public List<string> Check(SeasonalAdjustmentProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentProposalCodePrefixRule(string Prefix) : ISeasonalAdjustmentProposalRule
{
    public List<string> Check(SeasonalAdjustmentProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentProposalCodeLengthRule(int MaxLength) : ISeasonalAdjustmentProposalRule
{
    public List<string> Check(SeasonalAdjustmentProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal adjustment proposal.</summary>
public sealed record SeasonalAdjustmentProposalValidator(List<ISeasonalAdjustmentProposalRule> Rules)
{
    public List<string> Validate(SeasonalAdjustmentProposal item)
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

    public void RequireValid(SeasonalAdjustmentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAdjustmentProposal> items)
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

/// <summary>Functions of seasonal adjustment proposal rules.</summary>
public static class SeasonalAdjustmentProposalRules
{
    public static SeasonalAdjustmentProposalValidator DefaultSeasonalAdjustmentProposalValidator()
    {
        return new SeasonalAdjustmentProposalValidator(
            new List<ISeasonalAdjustmentProposalRule>
            {
                new SeasonalAdjustmentProposalRequiredCodeRule("seasonal adjustment proposal needs an entry"),
                new SeasonalAdjustmentProposalRequiredLabelRule("seasonal adjustment proposal needs an entry"),
                new SeasonalAdjustmentProposalCodePrefixRule("OPS"),
                new SeasonalAdjustmentProposalCodeLengthRule(3600),
            });
    }
}
