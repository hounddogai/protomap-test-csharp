namespace Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>A rule a action plan manifest must pass. Check returns its problems, or none.</summary>
public interface IActionPlanManifestRule
{
    List<string> Check(ActionPlanManifest item);
}

public sealed record ActionPlanManifestRevisionRangeRule(int Min, int Max) : IActionPlanManifestRule
{
    public List<string> Check(ActionPlanManifest item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanManifestLocationLengthRule(int MaxLength) : IActionPlanManifestRule
{
    public List<string> Check(ActionPlanManifest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanManifestRequiredLocationRule(string Message) : IActionPlanManifestRule
{
    public List<string> Check(ActionPlanManifest item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanManifestTitleLengthRule(int MaxLength) : IActionPlanManifestRule
{
    public List<string> Check(ActionPlanManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan manifest.</summary>
public sealed record ActionPlanManifestValidator(List<IActionPlanManifestRule> Rules)
{
    public List<string> Validate(ActionPlanManifest item)
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

    public void RequireValid(ActionPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanManifest> items)
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

/// <summary>Functions of action plan manifest rules.</summary>
public static class ActionPlanManifestRules
{
    public static ActionPlanManifestValidator DefaultActionPlanManifestValidator()
    {
        return new ActionPlanManifestValidator(
            new List<IActionPlanManifestRule>
            {
                new ActionPlanManifestRevisionRangeRule(64, 120),
                new ActionPlanManifestLocationLengthRule(3600),
                new ActionPlanManifestRequiredLocationRule("action plan manifest needs a value"),
                new ActionPlanManifestTitleLengthRule(64),
            });
    }
}
