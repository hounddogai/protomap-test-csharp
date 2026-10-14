namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default return plan manifest must pass. Check returns its problems, or none.</summary>
public interface IDefaultReturnPlanManifestRule
{
    List<string> Check(DefaultReturnPlanManifest item);
}

public sealed record DefaultReturnPlanManifestOwnerIdLengthRule(int MaxLength) : IDefaultReturnPlanManifestRule
{
    public List<string> Check(DefaultReturnPlanManifest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanManifestScoreRangeRule(double Min, double Max) : IDefaultReturnPlanManifestRule
{
    public List<string> Check(DefaultReturnPlanManifest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultReturnPlanManifestRequiredOwnerIdRule(string Message) : IDefaultReturnPlanManifestRule
{
    public List<string> Check(DefaultReturnPlanManifest item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanManifestAllowedTeamRule(List<string> Allowed) : IDefaultReturnPlanManifestRule
{
    public List<string> Check(DefaultReturnPlanManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanManifestRatioRangeRule(double Min, double Max) : IDefaultReturnPlanManifestRule
{
    public List<string> Check(DefaultReturnPlanManifest item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default return plan manifest.</summary>
public sealed record DefaultReturnPlanManifestValidator(List<IDefaultReturnPlanManifestRule> Rules)
{
    public List<string> Validate(DefaultReturnPlanManifest item)
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

    public void RequireValid(DefaultReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReturnPlanManifest> items)
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

/// <summary>Functions of default return plan manifest rules.</summary>
public static class DefaultReturnPlanManifestRules
{
    public static DefaultReturnPlanManifestValidator DefaultDefaultReturnPlanManifestValidator()
    {
        return new DefaultReturnPlanManifestValidator(
            new List<IDefaultReturnPlanManifestRule>
            {
                new DefaultReturnPlanManifestOwnerIdLengthRule(255),
                new DefaultReturnPlanManifestScoreRangeRule(2.3, 3814.1),
                new DefaultReturnPlanManifestRequiredOwnerIdRule("default return plan manifest needs a value"),
                new DefaultReturnPlanManifestAllowedTeamRule(new List<string> { "batched" }),
                new DefaultReturnPlanManifestRatioRangeRule(5.8, 289.9),
            });
    }
}
