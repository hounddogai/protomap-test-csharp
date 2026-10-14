namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive asset return allocation must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAssetReturnAllocationRule
{
    List<string> Check(AdaptiveAssetReturnAllocation item);
}

public sealed record AdaptiveAssetReturnAllocationRequiredLabelRule(string Message) : IAdaptiveAssetReturnAllocationRule
{
    public List<string> Check(AdaptiveAssetReturnAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnAllocationBatchSizeRangeRule(int Min, int Max) : IAdaptiveAssetReturnAllocationRule
{
    public List<string> Check(AdaptiveAssetReturnAllocation item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnAllocationAllowedRegionRule(List<string> Allowed) : IAdaptiveAssetReturnAllocationRule
{
    public List<string> Check(AdaptiveAssetReturnAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnAllocationJobCodeLengthRule(int MaxLength) : IAdaptiveAssetReturnAllocationRule
{
    public List<string> Check(AdaptiveAssetReturnAllocation item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive asset return allocation.</summary>
public sealed record AdaptiveAssetReturnAllocationValidator(List<IAdaptiveAssetReturnAllocationRule> Rules)
{
    public List<string> Validate(AdaptiveAssetReturnAllocation item)
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

    public void RequireValid(AdaptiveAssetReturnAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAssetReturnAllocation> items)
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

/// <summary>Functions of adaptive asset return allocation rules.</summary>
public static class AdaptiveAssetReturnAllocationRules
{
    public static AdaptiveAssetReturnAllocationValidator DefaultAdaptiveAssetReturnAllocationValidator()
    {
        return new AdaptiveAssetReturnAllocationValidator(
            new List<IAdaptiveAssetReturnAllocationRule>
            {
                new AdaptiveAssetReturnAllocationRequiredLabelRule("adaptive asset return allocation needs an entry"),
                new AdaptiveAssetReturnAllocationBatchSizeRangeRule(16, 255),
                new AdaptiveAssetReturnAllocationAllowedRegionRule(new List<string> { "weighted" }),
                new AdaptiveAssetReturnAllocationJobCodeLengthRule(80),
            });
    }
}
