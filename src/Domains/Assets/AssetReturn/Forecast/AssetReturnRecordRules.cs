namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return record must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnRecordRule
{
    List<string> Check(AssetReturnRecord item);
}

public sealed record AssetReturnRecordRequiredCountryRule(string Message) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRecordRequiredLabelRule(string Message) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRecordGradeLengthRule(int MaxLength) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRecordHoursRangeRule(double Min, double Max) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
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

public sealed record AssetReturnRecordRequiredCodeRule(string Message) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnRecordAllowedManagerIdRule(List<string> Allowed) : IAssetReturnRecordRule
{
    public List<string> Check(AssetReturnRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return record.</summary>
public sealed record AssetReturnRecordValidator(List<IAssetReturnRecordRule> Rules)
{
    public List<string> Validate(AssetReturnRecord item)
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

    public void RequireValid(AssetReturnRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnRecord> items)
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

/// <summary>Functions of asset return record rules.</summary>
public static class AssetReturnRecordRules
{
    public static AssetReturnRecordValidator DefaultAssetReturnRecordValidator()
    {
        return new AssetReturnRecordValidator(
            new List<IAssetReturnRecordRule>
            {
                new AssetReturnRecordRequiredCountryRule("asset return record needs an entry"),
                new AssetReturnRecordRequiredLabelRule("asset return record needs an entry"),
                new AssetReturnRecordGradeLengthRule(120),
                new AssetReturnRecordHoursRangeRule(6.2, 3354.7),
                new AssetReturnRecordRequiredCodeRule("asset return record needs an entry"),
                new AssetReturnRecordAllowedManagerIdRule(new List<string> { "global", "manual", "legacy" }),
            });
    }
}
