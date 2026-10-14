namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict return plan bundle must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanBundleRule
{
    List<string> Check(StrictReturnPlanBundle item);
}

public sealed record StrictReturnPlanBundleRequiredNameRule(string Message) : IStrictReturnPlanBundleRule
{
    public List<string> Check(StrictReturnPlanBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBundleVersionRangeRule(int Min, int Max) : IStrictReturnPlanBundleRule
{
    public List<string> Check(StrictReturnPlanBundle item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictReturnPlanBundleApprovedGuardRule(int Limit) : IStrictReturnPlanBundleRule
{
    public List<string> Check(StrictReturnPlanBundle item)
    {
        if (item.Approved && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBundleAllowedCodeRule(List<string> Allowed) : IStrictReturnPlanBundleRule
{
    public List<string> Check(StrictReturnPlanBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBundleRequiredTitleRule(string Message) : IStrictReturnPlanBundleRule
{
    public List<string> Check(StrictReturnPlanBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan bundle.</summary>
public sealed record StrictReturnPlanBundleValidator(List<IStrictReturnPlanBundleRule> Rules)
{
    public List<string> Validate(StrictReturnPlanBundle item)
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

    public void RequireValid(StrictReturnPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanBundle> items)
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

/// <summary>Functions of strict return plan bundle rules.</summary>
public static class StrictReturnPlanBundleRules
{
    public static StrictReturnPlanBundleValidator DefaultStrictReturnPlanBundleValidator()
    {
        return new StrictReturnPlanBundleValidator(
            new List<IStrictReturnPlanBundleRule>
            {
                new StrictReturnPlanBundleRequiredNameRule("strict return plan bundle needs a value"),
                new StrictReturnPlanBundleVersionRangeRule(16, 120),
                new StrictReturnPlanBundleApprovedGuardRule(64),
                new StrictReturnPlanBundleAllowedCodeRule(new List<string> { "legacy", "batched" }),
                new StrictReturnPlanBundleRequiredTitleRule("strict return plan bundle needs a value"),
            });
    }
}
