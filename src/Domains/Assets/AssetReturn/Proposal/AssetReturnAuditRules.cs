namespace Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>A rule a asset return audit must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnAuditRule
{
    List<string> Check(AssetReturnAudit item);
}

public sealed record AssetReturnAuditNameLengthRule(int MaxLength) : IAssetReturnAuditRule
{
    public List<string> Check(AssetReturnAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAuditRequiredSkillRule(string Message) : IAssetReturnAuditRule
{
    public List<string> Check(AssetReturnAudit item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAuditLeaveDaysRangeRule(int Min, int Max) : IAssetReturnAuditRule
{
    public List<string> Check(AssetReturnAudit item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAuditVisibleGuardRule(double Limit) : IAssetReturnAuditRule
{
    public List<string> Check(AssetReturnAudit item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return audit.</summary>
public sealed record AssetReturnAuditValidator(List<IAssetReturnAuditRule> Rules)
{
    public List<string> Validate(AssetReturnAudit item)
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

    public void RequireValid(AssetReturnAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnAudit> items)
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

/// <summary>Functions of asset return audit rules.</summary>
public static class AssetReturnAuditRules
{
    public static AssetReturnAuditValidator DefaultAssetReturnAuditValidator()
    {
        return new AssetReturnAuditValidator(
            new List<IAssetReturnAuditRule>
            {
                new AssetReturnAuditNameLengthRule(255),
                new AssetReturnAuditRequiredSkillRule("asset return audit needs an entry"),
                new AssetReturnAuditLeaveDaysRangeRule(16, 120),
                new AssetReturnAuditVisibleGuardRule(1.4),
            });
    }
}
