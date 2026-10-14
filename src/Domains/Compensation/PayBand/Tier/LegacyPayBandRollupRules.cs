namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy pay band rollup must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayBandRollupRule
{
    List<string> Check(LegacyPayBandRollup item);
}

public sealed record LegacyPayBandRollupRequiredNameRule(string Message) : ILegacyPayBandRollupRule
{
    public List<string> Check(LegacyPayBandRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRollupAllowedNameRule(List<string> Allowed) : ILegacyPayBandRollupRule
{
    public List<string> Check(LegacyPayBandRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRollupRequiredCodeRule(string Message) : ILegacyPayBandRollupRule
{
    public List<string> Check(LegacyPayBandRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay band rollup.</summary>
public sealed record LegacyPayBandRollupValidator(List<ILegacyPayBandRollupRule> Rules)
{
    public List<string> Validate(LegacyPayBandRollup item)
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

    public void RequireValid(LegacyPayBandRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayBandRollup> items)
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

/// <summary>Functions of legacy pay band rollup rules.</summary>
public static class LegacyPayBandRollupRules
{
    public static LegacyPayBandRollupValidator DefaultLegacyPayBandRollupValidator()
    {
        return new LegacyPayBandRollupValidator(
            new List<ILegacyPayBandRollupRule>
            {
                new LegacyPayBandRollupRequiredNameRule("legacy pay band rollup needs this field"),
                new LegacyPayBandRollupAllowedNameRule(new List<string> { "tiered", "rolling" }),
                new LegacyPayBandRollupRequiredCodeRule("legacy pay band rollup needs an entry"),
            });
    }
}
