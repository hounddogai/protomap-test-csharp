namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>A rule a manual exception manifest must pass. Check returns its problems, or none.</summary>
public interface IManualExceptionManifestRule
{
    List<string> Check(ManualExceptionManifest item);
}

public sealed record ManualExceptionManifestPinnedGuardRule(int Limit) : IManualExceptionManifestRule
{
    public List<string> Check(ManualExceptionManifest item)
    {
        if (item.Pinned && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionManifestNameLengthRule(int MaxLength) : IManualExceptionManifestRule
{
    public List<string> Check(ManualExceptionManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionManifestCostCenterLengthRule(int MaxLength) : IManualExceptionManifestRule
{
    public List<string> Check(ManualExceptionManifest item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionManifestChannelLengthRule(int MaxLength) : IManualExceptionManifestRule
{
    public List<string> Check(ManualExceptionManifest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual exception manifest.</summary>
public sealed record ManualExceptionManifestValidator(List<IManualExceptionManifestRule> Rules)
{
    public List<string> Validate(ManualExceptionManifest item)
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

    public void RequireValid(ManualExceptionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualExceptionManifest> items)
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

/// <summary>Functions of manual exception manifest rules.</summary>
public static class ManualExceptionManifestRules
{
    public static ManualExceptionManifestValidator DefaultManualExceptionManifestValidator()
    {
        return new ManualExceptionManifestValidator(
            new List<IManualExceptionManifestRule>
            {
                new ManualExceptionManifestPinnedGuardRule(255),
                new ManualExceptionManifestNameLengthRule(80),
                new ManualExceptionManifestCostCenterLengthRule(32),
                new ManualExceptionManifestChannelLengthRule(80),
            });
    }
}
