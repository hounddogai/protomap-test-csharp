namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>A rule a bonus pool manifest must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolManifestRule
{
    List<string> Check(BonusPoolManifest item);
}

public sealed record BonusPoolManifestSourceLengthRule(int MaxLength) : IBonusPoolManifestRule
{
    public List<string> Check(BonusPoolManifest item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolManifestRequiredCodeRule(string Message) : IBonusPoolManifestRule
{
    public List<string> Check(BonusPoolManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolManifestRequiredJobCodeRule(string Message) : IBonusPoolManifestRule
{
    public List<string> Check(BonusPoolManifest item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool manifest.</summary>
public sealed record BonusPoolManifestValidator(List<IBonusPoolManifestRule> Rules)
{
    public List<string> Validate(BonusPoolManifest item)
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

    public void RequireValid(BonusPoolManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolManifest> items)
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

/// <summary>Functions of bonus pool manifest rules.</summary>
public static class BonusPoolManifestRules
{
    public static BonusPoolManifestValidator DefaultBonusPoolManifestValidator()
    {
        return new BonusPoolManifestValidator(
            new List<IBonusPoolManifestRule>
            {
                new BonusPoolManifestSourceLengthRule(1000),
                new BonusPoolManifestRequiredCodeRule("bonus pool manifest needs an entry"),
                new BonusPoolManifestRequiredJobCodeRule("bonus pool manifest needs a value"),
            });
    }
}
