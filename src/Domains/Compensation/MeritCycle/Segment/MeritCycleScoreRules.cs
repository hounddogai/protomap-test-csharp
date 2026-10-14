namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle score must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleScoreRule
{
    List<string> Check(MeritCycleScore item);
}

public sealed record MeritCycleScoreRequiredCodeRule(string Message) : IMeritCycleScoreRule
{
    public List<string> Check(MeritCycleScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleScoreUpdatedAtRangeRule(int Min, int Max) : IMeritCycleScoreRule
{
    public List<string> Check(MeritCycleScore item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleScoreMarginRangeRule(double Min, double Max) : IMeritCycleScoreRule
{
    public List<string> Check(MeritCycleScore item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleScoreAllowedJobCodeRule(List<string> Allowed) : IMeritCycleScoreRule
{
    public List<string> Check(MeritCycleScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleScoreRequiredNameRule(string Message) : IMeritCycleScoreRule
{
    public List<string> Check(MeritCycleScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle score.</summary>
public sealed record MeritCycleScoreValidator(List<IMeritCycleScoreRule> Rules)
{
    public List<string> Validate(MeritCycleScore item)
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

    public void RequireValid(MeritCycleScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleScore> items)
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

/// <summary>Functions of merit cycle score rules.</summary>
public static class MeritCycleScoreRules
{
    public static MeritCycleScoreValidator DefaultMeritCycleScoreValidator()
    {
        return new MeritCycleScoreValidator(
            new List<IMeritCycleScoreRule>
            {
                new MeritCycleScoreRequiredCodeRule("merit cycle score needs this field"),
                new MeritCycleScoreUpdatedAtRangeRule(120, 255),
                new MeritCycleScoreMarginRangeRule(5.4, 904.9),
                new MeritCycleScoreAllowedJobCodeRule(new List<string> { "regional" }),
                new MeritCycleScoreRequiredNameRule("merit cycle score needs an entry"),
            });
    }
}
