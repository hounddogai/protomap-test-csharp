namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>A rule a fallback succession plan manifest must pass. Check returns its problems, or none.</summary>
public interface IFallbackSuccessionPlanManifestRule
{
    List<string> Check(FallbackSuccessionPlanManifest item);
}

public sealed record FallbackSuccessionPlanManifestWindowDaysRangeRule(int Min, int Max) : IFallbackSuccessionPlanManifestRule
{
    public List<string> Check(FallbackSuccessionPlanManifest item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSuccessionPlanManifestCodeLengthRule(int MaxLength) : IFallbackSuccessionPlanManifestRule
{
    public List<string> Check(FallbackSuccessionPlanManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanManifestVersionRangeRule(int Min, int Max) : IFallbackSuccessionPlanManifestRule
{
    public List<string> Check(FallbackSuccessionPlanManifest item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback succession plan manifest.</summary>
public sealed record FallbackSuccessionPlanManifestValidator(List<IFallbackSuccessionPlanManifestRule> Rules)
{
    public List<string> Validate(FallbackSuccessionPlanManifest item)
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

    public void RequireValid(FallbackSuccessionPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSuccessionPlanManifest> items)
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

/// <summary>Functions of fallback succession plan manifest rules.</summary>
public static class FallbackSuccessionPlanManifestRules
{
    public static FallbackSuccessionPlanManifestValidator DefaultFallbackSuccessionPlanManifestValidator()
    {
        return new FallbackSuccessionPlanManifestValidator(
            new List<IFallbackSuccessionPlanManifestRule>
            {
                new FallbackSuccessionPlanManifestWindowDaysRangeRule(1000, 3600),
                new FallbackSuccessionPlanManifestCodeLengthRule(3600),
                new FallbackSuccessionPlanManifestVersionRangeRule(16, 64),
            });
    }
}
