namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict audit manifest must pass. Check returns its problems, or none.</summary>
public interface IStrictAuditManifestRule
{
    List<string> Check(StrictAuditManifest item);
}

public sealed record StrictAuditManifestActiveGuardRule(double Limit) : IStrictAuditManifestRule
{
    public List<string> Check(StrictAuditManifest item)
    {
        if (item.Active && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditManifestAllowedCodeRule(List<string> Allowed) : IStrictAuditManifestRule
{
    public List<string> Check(StrictAuditManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record StrictAuditManifestMinimumRangeRule(double Min, double Max) : IStrictAuditManifestRule
{
    public List<string> Check(StrictAuditManifest item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict audit manifest.</summary>
public sealed record StrictAuditManifestValidator(List<IStrictAuditManifestRule> Rules)
{
    public List<string> Validate(StrictAuditManifest item)
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

    public void RequireValid(StrictAuditManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAuditManifest> items)
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

/// <summary>Functions of strict audit manifest rules.</summary>
public static class StrictAuditManifestRules
{
    public static StrictAuditManifestValidator DefaultStrictAuditManifestValidator()
    {
        return new StrictAuditManifestValidator(
            new List<IStrictAuditManifestRule>
            {
                new StrictAuditManifestActiveGuardRule(0.7),
                new StrictAuditManifestAllowedCodeRule(new List<string> { "tiered", "default", "batched" }),
                new StrictAuditManifestMinimumRangeRule(1.7, 1994.7),
            });
    }
}
