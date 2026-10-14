namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default cost model manifest must pass. Check returns its problems, or none.</summary>
public interface IDefaultCostModelManifestRule
{
    List<string> Check(DefaultCostModelManifest item);
}

public sealed record DefaultCostModelManifestRequiredTitleRule(string Message) : IDefaultCostModelManifestRule
{
    public List<string> Check(DefaultCostModelManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultCostModelManifestRetriesRangeRule(int Min, int Max) : IDefaultCostModelManifestRule
{
    public List<string> Check(DefaultCostModelManifest item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultCostModelManifestAllowedTitleRule(List<string> Allowed) : IDefaultCostModelManifestRule
{
    public List<string> Check(DefaultCostModelManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCostModelManifestRequiredExternalRefRule(string Message) : IDefaultCostModelManifestRule
{
    public List<string> Check(DefaultCostModelManifest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultCostModelManifestCodePrefixRule(string Prefix) : IDefaultCostModelManifestRule
{
    public List<string> Check(DefaultCostModelManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default cost model manifest.</summary>
public sealed record DefaultCostModelManifestValidator(List<IDefaultCostModelManifestRule> Rules)
{
    public List<string> Validate(DefaultCostModelManifest item)
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

    public void RequireValid(DefaultCostModelManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultCostModelManifest> items)
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

/// <summary>Functions of default cost model manifest rules.</summary>
public static class DefaultCostModelManifestRules
{
    public static DefaultCostModelManifestValidator DefaultDefaultCostModelManifestValidator()
    {
        return new DefaultCostModelManifestValidator(
            new List<IDefaultCostModelManifestRule>
            {
                new DefaultCostModelManifestRequiredTitleRule("default cost model manifest needs an entry"),
                new DefaultCostModelManifestRetriesRangeRule(64, 120),
                new DefaultCostModelManifestAllowedTitleRule(new List<string>()),
                new DefaultCostModelManifestRequiredExternalRefRule("default cost model manifest needs a value"),
                new DefaultCostModelManifestCodePrefixRule("X"),
            });
    }
}
