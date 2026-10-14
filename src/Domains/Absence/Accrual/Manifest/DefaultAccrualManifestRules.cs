namespace Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>A rule a default accrual manifest must pass. Check returns its problems, or none.</summary>
public interface IDefaultAccrualManifestRule
{
    List<string> Check(DefaultAccrualManifest item);
}

public sealed record DefaultAccrualManifestSequenceRangeRule(int Min, int Max) : IDefaultAccrualManifestRule
{
    public List<string> Check(DefaultAccrualManifest item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultAccrualManifestCodePrefixRule(string Prefix) : IDefaultAccrualManifestRule
{
    public List<string> Check(DefaultAccrualManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccrualManifestRequiredTimezoneRule(string Message) : IDefaultAccrualManifestRule
{
    public List<string> Check(DefaultAccrualManifest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default accrual manifest.</summary>
public sealed record DefaultAccrualManifestValidator(List<IDefaultAccrualManifestRule> Rules)
{
    public List<string> Validate(DefaultAccrualManifest item)
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

    public void RequireValid(DefaultAccrualManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAccrualManifest> items)
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

/// <summary>Functions of default accrual manifest rules.</summary>
public static class DefaultAccrualManifestRules
{
    public static DefaultAccrualManifestValidator DefaultDefaultAccrualManifestValidator()
    {
        return new DefaultAccrualManifestValidator(
            new List<IDefaultAccrualManifestRule>
            {
                new DefaultAccrualManifestSequenceRangeRule(64, 255),
                new DefaultAccrualManifestCodePrefixRule("Q"),
                new DefaultAccrualManifestRequiredTimezoneRule("default accrual manifest needs an entry"),
            });
    }
}
