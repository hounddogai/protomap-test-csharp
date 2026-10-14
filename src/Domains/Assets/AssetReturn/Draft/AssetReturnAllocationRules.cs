namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return allocation must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnAllocationRule
{
    List<string> Check(AssetReturnAllocation item);
}

public sealed record AssetReturnAllocationMaximumRangeRule(double Min, double Max) : IAssetReturnAllocationRule
{
    public List<string> Check(AssetReturnAllocation item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnAllocationAllowedDescriptionRule(List<string> Allowed) : IAssetReturnAllocationRule
{
    public List<string> Check(AssetReturnAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAllocationRequiredDescriptionRule(string Message) : IAssetReturnAllocationRule
{
    public List<string> Check(AssetReturnAllocation item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAllocationTitleLengthRule(int MaxLength) : IAssetReturnAllocationRule
{
    public List<string> Check(AssetReturnAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAllocationDescriptionLengthRule(int MaxLength) : IAssetReturnAllocationRule
{
    public List<string> Check(AssetReturnAllocation item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return allocation.</summary>
public sealed record AssetReturnAllocationValidator(List<IAssetReturnAllocationRule> Rules)
{
    public List<string> Validate(AssetReturnAllocation item)
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

    public void RequireValid(AssetReturnAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnAllocation> items)
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

/// <summary>Functions of asset return allocation rules.</summary>
public static class AssetReturnAllocationRules
{
    public static AssetReturnAllocationValidator DefaultAssetReturnAllocationValidator()
    {
        return new AssetReturnAllocationValidator(
            new List<IAssetReturnAllocationRule>
            {
                new AssetReturnAllocationMaximumRangeRule(4.9, 2546.0),
                new AssetReturnAllocationAllowedDescriptionRule(
                    new List<string> { "seasonal", "lenient", "rolling" }),
                new AssetReturnAllocationRequiredDescriptionRule("asset return allocation needs this field"),
                new AssetReturnAllocationTitleLengthRule(3600),
                new AssetReturnAllocationDescriptionLengthRule(80),
            });
    }
}
