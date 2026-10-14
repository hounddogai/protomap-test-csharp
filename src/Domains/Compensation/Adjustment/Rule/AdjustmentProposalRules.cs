namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>A rule a adjustment proposal must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentProposalRule
{
    List<string> Check(AdjustmentProposal item);
}

public sealed record AdjustmentProposalRequiredLabelRule(string Message) : IAdjustmentProposalRule
{
    public List<string> Check(AdjustmentProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentProposalCodePrefixRule(string Prefix) : IAdjustmentProposalRule
{
    public List<string> Check(AdjustmentProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentProposalStartsAtRangeRule(int Min, int Max) : IAdjustmentProposalRule
{
    public List<string> Check(AdjustmentProposal item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adjustment proposal.</summary>
public sealed record AdjustmentProposalValidator(List<IAdjustmentProposalRule> Rules)
{
    public List<string> Validate(AdjustmentProposal item)
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

    public void RequireValid(AdjustmentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentProposal> items)
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

/// <summary>Functions of adjustment proposal rules.</summary>
public static class AdjustmentProposalRules
{
    public static AdjustmentProposalValidator DefaultAdjustmentProposalValidator()
    {
        return new AdjustmentProposalValidator(
            new List<IAdjustmentProposalRule>
            {
                new AdjustmentProposalRequiredLabelRule("adjustment proposal needs an entry"),
                new AdjustmentProposalCodePrefixRule("ACM"),
                new AdjustmentProposalStartsAtRangeRule(32, 1000),
            });
    }
}
