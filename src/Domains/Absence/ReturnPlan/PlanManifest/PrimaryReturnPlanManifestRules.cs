namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>A rule a primary return plan manifest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryReturnPlanManifestRule
{
    List<string> Check(PrimaryReturnPlanManifest item);
}

public sealed record PrimaryReturnPlanManifestRequiredTimezoneRule(string Message) : IPrimaryReturnPlanManifestRule
{
    public List<string> Check(PrimaryReturnPlanManifest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReturnPlanManifestCodeLengthRule(int MaxLength) : IPrimaryReturnPlanManifestRule
{
    public List<string> Check(PrimaryReturnPlanManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReturnPlanManifestTitleLengthRule(int MaxLength) : IPrimaryReturnPlanManifestRule
{
    public List<string> Check(PrimaryReturnPlanManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReturnPlanManifestRequiredTitleRule(string Message) : IPrimaryReturnPlanManifestRule
{
    public List<string> Check(PrimaryReturnPlanManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary return plan manifest.</summary>
public sealed record PrimaryReturnPlanManifestValidator(List<IPrimaryReturnPlanManifestRule> Rules)
{
    public List<string> Validate(PrimaryReturnPlanManifest item)
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

    public void RequireValid(PrimaryReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryReturnPlanManifest> items)
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

/// <summary>Functions of primary return plan manifest rules.</summary>
public static class PrimaryReturnPlanManifestRules
{
    public static PrimaryReturnPlanManifestValidator DefaultPrimaryReturnPlanManifestValidator()
    {
        return new PrimaryReturnPlanManifestValidator(
            new List<IPrimaryReturnPlanManifestRule>
            {
                new PrimaryReturnPlanManifestRequiredTimezoneRule("primary return plan manifest needs an entry"),
                new PrimaryReturnPlanManifestCodeLengthRule(3600),
                new PrimaryReturnPlanManifestTitleLengthRule(32),
                new PrimaryReturnPlanManifestRequiredTitleRule("primary return plan manifest needs an entry"),
            });
    }
}
