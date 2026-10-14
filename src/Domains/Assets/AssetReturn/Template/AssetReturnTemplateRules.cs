namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>A rule a asset return template must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnTemplateRule
{
    List<string> Check(AssetReturnTemplate item);
}

public sealed record AssetReturnTemplateCodeLengthRule(int MaxLength) : IAssetReturnTemplateRule
{
    public List<string> Check(AssetReturnTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnTemplateSegmentLengthRule(int MaxLength) : IAssetReturnTemplateRule
{
    public List<string> Check(AssetReturnTemplate item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnTemplateRequiredLabelRule(string Message) : IAssetReturnTemplateRule
{
    public List<string> Check(AssetReturnTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return template.</summary>
public sealed record AssetReturnTemplateValidator(List<IAssetReturnTemplateRule> Rules)
{
    public List<string> Validate(AssetReturnTemplate item)
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

    public void RequireValid(AssetReturnTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnTemplate> items)
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

/// <summary>Functions of asset return template rules.</summary>
public static class AssetReturnTemplateRules
{
    public static AssetReturnTemplateValidator DefaultAssetReturnTemplateValidator()
    {
        return new AssetReturnTemplateValidator(
            new List<IAssetReturnTemplateRule>
            {
                new AssetReturnTemplateCodeLengthRule(3600),
                new AssetReturnTemplateSegmentLengthRule(86400),
                new AssetReturnTemplateRequiredLabelRule("asset return template needs a value"),
            });
    }
}
