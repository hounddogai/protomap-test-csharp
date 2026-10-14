namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg score must pass. Check returns its problems, or none.</summary>
public interface IReorgScoreRule
{
    List<string> Check(ReorgScore item);
}

public sealed record ReorgScoreRequiredCodeRule(string Message) : IReorgScoreRule
{
    public List<string> Check(ReorgScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgScoreFloorValueRangeRule(double Min, double Max) : IReorgScoreRule
{
    public List<string> Check(ReorgScore item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReorgScoreAllowedCodeRule(List<string> Allowed) : IReorgScoreRule
{
    public List<string> Check(ReorgScore item)
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

/// <summary>Runs every rule of a reorg score.</summary>
public sealed record ReorgScoreValidator(List<IReorgScoreRule> Rules)
{
    public List<string> Validate(ReorgScore item)
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

    public void RequireValid(ReorgScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgScore> items)
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

/// <summary>Functions of reorg score rules.</summary>
public static class ReorgScoreRules
{
    public static ReorgScoreValidator DefaultReorgScoreValidator()
    {
        return new ReorgScoreValidator(
            new List<IReorgScoreRule>
            {
                new ReorgScoreRequiredCodeRule("reorg score needs a value"),
                new ReorgScoreFloorValueRangeRule(3.0, 3683.3),
                new ReorgScoreAllowedCodeRule(new List<string> { "manual" }),
            });
    }
}
