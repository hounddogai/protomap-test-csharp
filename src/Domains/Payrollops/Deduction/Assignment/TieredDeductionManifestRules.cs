namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered deduction manifest must pass. Check returns its problems, or none.</summary>
public interface ITieredDeductionManifestRule
{
    List<string> Check(TieredDeductionManifest item);
}

public sealed record TieredDeductionManifestAllowedCodeRule(List<string> Allowed) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeductionManifestDurationMinutesRangeRule(int Min, int Max) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeductionManifestRequiredCodeRule(string Message) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredDeductionManifestCodePrefixRule(string Prefix) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeductionManifestRequiredTeamRule(string Message) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredDeductionManifestFloorValueRangeRule(double Min, double Max) : ITieredDeductionManifestRule
{
    public List<string> Check(TieredDeductionManifest item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered deduction manifest.</summary>
public sealed record TieredDeductionManifestValidator(List<ITieredDeductionManifestRule> Rules)
{
    public List<string> Validate(TieredDeductionManifest item)
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

    public void RequireValid(TieredDeductionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredDeductionManifest> items)
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

/// <summary>Functions of tiered deduction manifest rules.</summary>
public static class TieredDeductionManifestRules
{
    public static TieredDeductionManifestValidator DefaultTieredDeductionManifestValidator()
    {
        return new TieredDeductionManifestValidator(
            new List<ITieredDeductionManifestRule>
            {
                new TieredDeductionManifestAllowedCodeRule(new List<string> { "legacy", "legacy" }),
                new TieredDeductionManifestDurationMinutesRangeRule(64, 255),
                new TieredDeductionManifestRequiredCodeRule("tiered deduction manifest needs a value"),
                new TieredDeductionManifestCodePrefixRule("REF"),
                new TieredDeductionManifestRequiredTeamRule("tiered deduction manifest needs this field"),
                new TieredDeductionManifestFloorValueRangeRule(2.7, 3767.1),
            });
    }
}
