namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default desk score must pass. Check returns its problems, or none.</summary>
public interface IDefaultDeskScoreRule
{
    List<string> Check(DefaultDeskScore item);
}

public sealed record DefaultDeskScoreAllowedEmployeeIdRule(List<string> Allowed) : IDefaultDeskScoreRule
{
    public List<string> Check(DefaultDeskScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskScoreRequiredLabelRule(string Message) : IDefaultDeskScoreRule
{
    public List<string> Check(DefaultDeskScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskScoreRequiredEmployeeIdRule(string Message) : IDefaultDeskScoreRule
{
    public List<string> Check(DefaultDeskScore item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default desk score.</summary>
public sealed record DefaultDeskScoreValidator(List<IDefaultDeskScoreRule> Rules)
{
    public List<string> Validate(DefaultDeskScore item)
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

    public void RequireValid(DefaultDeskScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultDeskScore> items)
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

/// <summary>Functions of default desk score rules.</summary>
public static class DefaultDeskScoreRules
{
    public static DefaultDeskScoreValidator DefaultDefaultDeskScoreValidator()
    {
        return new DefaultDeskScoreValidator(
            new List<IDefaultDeskScoreRule>
            {
                new DefaultDeskScoreAllowedEmployeeIdRule(new List<string> { "primary", "weighted", "primary" }),
                new DefaultDeskScoreRequiredLabelRule("default desk score needs a value"),
                new DefaultDeskScoreRequiredEmployeeIdRule("default desk score needs this field"),
            });
    }
}
