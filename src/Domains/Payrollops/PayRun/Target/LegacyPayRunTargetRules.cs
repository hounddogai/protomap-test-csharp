namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy pay run target must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayRunTargetRule
{
    List<string> Check(LegacyPayRunTarget item);
}

public sealed record LegacyPayRunTargetCodeLengthRule(int MaxLength) : ILegacyPayRunTargetRule
{
    public List<string> Check(LegacyPayRunTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunTargetRequiredBadgeRule(string Message) : ILegacyPayRunTargetRule
{
    public List<string> Check(LegacyPayRunTarget item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunTargetAllowedCodeRule(List<string> Allowed) : ILegacyPayRunTargetRule
{
    public List<string> Check(LegacyPayRunTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunTargetRequiredNameRule(string Message) : ILegacyPayRunTargetRule
{
    public List<string> Check(LegacyPayRunTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay run target.</summary>
public sealed record LegacyPayRunTargetValidator(List<ILegacyPayRunTargetRule> Rules)
{
    public List<string> Validate(LegacyPayRunTarget item)
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

    public void RequireValid(LegacyPayRunTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayRunTarget> items)
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

/// <summary>Functions of legacy pay run target rules.</summary>
public static class LegacyPayRunTargetRules
{
    public static LegacyPayRunTargetValidator DefaultLegacyPayRunTargetValidator()
    {
        return new LegacyPayRunTargetValidator(
            new List<ILegacyPayRunTargetRule>
            {
                new LegacyPayRunTargetCodeLengthRule(1000),
                new LegacyPayRunTargetRequiredBadgeRule("legacy pay run target needs this field"),
                new LegacyPayRunTargetAllowedCodeRule(new List<string> { "tiered", "seasonal", "primary" }),
                new LegacyPayRunTargetRequiredNameRule("legacy pay run target needs a value"),
            });
    }
}
