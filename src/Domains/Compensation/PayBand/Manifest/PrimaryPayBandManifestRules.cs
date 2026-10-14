namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary pay band manifest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPayBandManifestRule
{
    List<string> Check(PrimaryPayBandManifest item);
}

public sealed record PrimaryPayBandManifestQuantityRangeRule(int Min, int Max) : IPrimaryPayBandManifestRule
{
    public List<string> Check(PrimaryPayBandManifest item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryPayBandManifestPinnedGuardRule(int Limit) : IPrimaryPayBandManifestRule
{
    public List<string> Check(PrimaryPayBandManifest item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandManifestAllowedCodeRule(List<string> Allowed) : IPrimaryPayBandManifestRule
{
    public List<string> Check(PrimaryPayBandManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary pay band manifest.</summary>
public sealed record PrimaryPayBandManifestValidator(List<IPrimaryPayBandManifestRule> Rules)
{
    public List<string> Validate(PrimaryPayBandManifest item)
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

    public void RequireValid(PrimaryPayBandManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPayBandManifest> items)
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

/// <summary>Functions of primary pay band manifest rules.</summary>
public static class PrimaryPayBandManifestRules
{
    public static PrimaryPayBandManifestValidator DefaultPrimaryPayBandManifestValidator()
    {
        return new PrimaryPayBandManifestValidator(
            new List<IPrimaryPayBandManifestRule>
            {
                new PrimaryPayBandManifestQuantityRangeRule(1000, 3600),
                new PrimaryPayBandManifestPinnedGuardRule(80),
                new PrimaryPayBandManifestAllowedCodeRule(new List<string>()),
            });
    }
}
