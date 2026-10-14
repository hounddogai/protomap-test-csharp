namespace Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>A rule a regional asset return proposal must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssetReturnProposalRule
{
    List<string> Check(RegionalAssetReturnProposal item);
}

public sealed record RegionalAssetReturnProposalTitleLengthRule(int MaxLength) : IRegionalAssetReturnProposalRule
{
    public List<string> Check(RegionalAssetReturnProposal item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnProposalRequiredTitleRule(string Message) : IRegionalAssetReturnProposalRule
{
    public List<string> Check(RegionalAssetReturnProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnProposalRequiredPhoneRule(string Message) : IRegionalAssetReturnProposalRule
{
    public List<string> Check(RegionalAssetReturnProposal item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnProposalRequiredCodeRule(string Message) : IRegionalAssetReturnProposalRule
{
    public List<string> Check(RegionalAssetReturnProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnProposalThresholdRangeRule(double Min, double Max) : IRegionalAssetReturnProposalRule
{
    public List<string> Check(RegionalAssetReturnProposal item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional asset return proposal.</summary>
public sealed record RegionalAssetReturnProposalValidator(List<IRegionalAssetReturnProposalRule> Rules)
{
    public List<string> Validate(RegionalAssetReturnProposal item)
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

    public void RequireValid(RegionalAssetReturnProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssetReturnProposal> items)
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

/// <summary>Functions of regional asset return proposal rules.</summary>
public static class RegionalAssetReturnProposalRules
{
    public static RegionalAssetReturnProposalValidator DefaultRegionalAssetReturnProposalValidator()
    {
        return new RegionalAssetReturnProposalValidator(
            new List<IRegionalAssetReturnProposalRule>
            {
                new RegionalAssetReturnProposalTitleLengthRule(255),
                new RegionalAssetReturnProposalRequiredTitleRule("regional asset return proposal needs a value"),
                new RegionalAssetReturnProposalRequiredPhoneRule("regional asset return proposal needs this field"),
                new RegionalAssetReturnProposalRequiredCodeRule("regional asset return proposal needs this field"),
                new RegionalAssetReturnProposalThresholdRangeRule(8.7, 3435.2),
            });
    }
}
