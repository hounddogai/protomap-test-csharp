namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>A rule a strict return plan manifest must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanManifestRule
{
    List<string> Check(StrictReturnPlanManifest item);
}

public sealed record StrictReturnPlanManifestRequiredTitleRule(string Message) : IStrictReturnPlanManifestRule
{
    public List<string> Check(StrictReturnPlanManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanManifestTitleLengthRule(int MaxLength) : IStrictReturnPlanManifestRule
{
    public List<string> Check(StrictReturnPlanManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanManifestLocationLengthRule(int MaxLength) : IStrictReturnPlanManifestRule
{
    public List<string> Check(StrictReturnPlanManifest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanManifestRequiredLocationRule(string Message) : IStrictReturnPlanManifestRule
{
    public List<string> Check(StrictReturnPlanManifest item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan manifest.</summary>
public sealed record StrictReturnPlanManifestValidator(List<IStrictReturnPlanManifestRule> Rules)
{
    public List<string> Validate(StrictReturnPlanManifest item)
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

    public void RequireValid(StrictReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanManifest> items)
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

/// <summary>Functions of strict return plan manifest rules.</summary>
public static class StrictReturnPlanManifestRules
{
    public static StrictReturnPlanManifestValidator DefaultStrictReturnPlanManifestValidator()
    {
        return new StrictReturnPlanManifestValidator(
            new List<IStrictReturnPlanManifestRule>
            {
                new StrictReturnPlanManifestRequiredTitleRule("strict return plan manifest needs this field"),
                new StrictReturnPlanManifestTitleLengthRule(3600),
                new StrictReturnPlanManifestLocationLengthRule(16),
                new StrictReturnPlanManifestRequiredLocationRule("strict return plan manifest needs a value"),
            });
    }
}
