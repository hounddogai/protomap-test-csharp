namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default action plan manifest must pass. Check returns its problems, or none.</summary>
public interface IDefaultActionPlanManifestRule
{
    List<string> Check(DefaultActionPlanManifest item);
}

public sealed record DefaultActionPlanManifestAllowedCodeRule(List<string> Allowed) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanManifestCodeLengthRule(int MaxLength) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanManifestRequiredCodeRule(string Message) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanManifestHoursRangeRule(double Min, double Max) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanManifestWindowDaysRangeRule(int Min, int Max) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
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

public sealed record DefaultActionPlanManifestRequiredNotesRule(string Message) : IDefaultActionPlanManifestRule
{
    public List<string> Check(DefaultActionPlanManifest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default action plan manifest.</summary>
public sealed record DefaultActionPlanManifestValidator(List<IDefaultActionPlanManifestRule> Rules)
{
    public List<string> Validate(DefaultActionPlanManifest item)
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

    public void RequireValid(DefaultActionPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultActionPlanManifest> items)
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

/// <summary>Functions of default action plan manifest rules.</summary>
public static class DefaultActionPlanManifestRules
{
    public static DefaultActionPlanManifestValidator DefaultDefaultActionPlanManifestValidator()
    {
        return new DefaultActionPlanManifestValidator(
            new List<IDefaultActionPlanManifestRule>
            {
                new DefaultActionPlanManifestAllowedCodeRule(new List<string> { "lenient", "rolling", "fallback" }),
                new DefaultActionPlanManifestCodeLengthRule(64),
                new DefaultActionPlanManifestRequiredCodeRule("default action plan manifest needs this field"),
                new DefaultActionPlanManifestHoursRangeRule(8.7, 2883.1),
                new DefaultActionPlanManifestWindowDaysRangeRule(120, 86400),
                new DefaultActionPlanManifestRequiredNotesRule("default action plan manifest needs a value"),
            });
    }
}
