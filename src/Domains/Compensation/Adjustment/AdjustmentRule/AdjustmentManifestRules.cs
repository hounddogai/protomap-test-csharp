namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment manifest must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentManifestRule
{
    List<string> Check(AdjustmentManifest item);
}

public sealed record AdjustmentManifestDurationMinutesRangeRule(int Min, int Max) : IAdjustmentManifestRule
{
    public List<string> Check(AdjustmentManifest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentManifestAllowedCodeRule(List<string> Allowed) : IAdjustmentManifestRule
{
    public List<string> Check(AdjustmentManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentManifestCodeLengthRule(int MaxLength) : IAdjustmentManifestRule
{
    public List<string> Check(AdjustmentManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment manifest.</summary>
public sealed record AdjustmentManifestValidator(List<IAdjustmentManifestRule> Rules)
{
    public List<string> Validate(AdjustmentManifest item)
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

    public void RequireValid(AdjustmentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentManifest> items)
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

/// <summary>Functions of adjustment manifest rules.</summary>
public static class AdjustmentManifestRules
{
    public static AdjustmentManifestValidator DefaultAdjustmentManifestValidator()
    {
        return new AdjustmentManifestValidator(
            new List<IAdjustmentManifestRule>
            {
                new AdjustmentManifestDurationMinutesRangeRule(80, 120),
                new AdjustmentManifestAllowedCodeRule(new List<string> { "default", "capped" }),
                new AdjustmentManifestCodeLengthRule(3600),
            });
    }
}
