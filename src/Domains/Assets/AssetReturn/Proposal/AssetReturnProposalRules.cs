namespace Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>A rule a asset return proposal must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnProposalRule
{
    List<string> Check(AssetReturnProposal item);
}

public sealed record AssetReturnProposalCodePrefixRule(string Prefix) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProposalStartsAtRangeRule(int Min, int Max) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
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

public sealed record AssetReturnProposalRequiredLabelRule(string Message) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProposalQuantityRangeRule(int Min, int Max) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProposalRequiredCodeRule(string Message) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnProposalLabelLengthRule(int MaxLength) : IAssetReturnProposalRule
{
    public List<string> Check(AssetReturnProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return proposal.</summary>
public sealed record AssetReturnProposalValidator(List<IAssetReturnProposalRule> Rules)
{
    public List<string> Validate(AssetReturnProposal item)
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

    public void RequireValid(AssetReturnProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnProposal> items)
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

/// <summary>Functions of asset return proposal rules.</summary>
public static class AssetReturnProposalRules
{
    public static AssetReturnProposalValidator DefaultAssetReturnProposalValidator()
    {
        return new AssetReturnProposalValidator(
            new List<IAssetReturnProposalRule>
            {
                new AssetReturnProposalCodePrefixRule("ACM"),
                new AssetReturnProposalStartsAtRangeRule(3600, 86400),
                new AssetReturnProposalRequiredLabelRule("asset return proposal needs a value"),
                new AssetReturnProposalQuantityRangeRule(32, 3600),
                new AssetReturnProposalRequiredCodeRule("asset return proposal needs this field"),
                new AssetReturnProposalLabelLengthRule(120),
            });
    }
}
