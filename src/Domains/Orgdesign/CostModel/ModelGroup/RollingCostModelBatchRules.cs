namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>A rule a rolling cost model batch must pass. Check returns its problems, or none.</summary>
public interface IRollingCostModelBatchRule
{
    List<string> Check(RollingCostModelBatch item);
}

public sealed record RollingCostModelBatchRequiredSkillRule(string Message) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelBatchRequiredTeamRule(string Message) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelBatchTeamLengthRule(int MaxLength) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelBatchSkillLengthRule(int MaxLength) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelBatchRequiredLabelRule(string Message) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelBatchCreatedAtRangeRule(int Min, int Max) : IRollingCostModelBatchRule
{
    public List<string> Check(RollingCostModelBatch item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling cost model batch.</summary>
public sealed record RollingCostModelBatchValidator(List<IRollingCostModelBatchRule> Rules)
{
    public List<string> Validate(RollingCostModelBatch item)
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

    public void RequireValid(RollingCostModelBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCostModelBatch> items)
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

/// <summary>Functions of rolling cost model batch rules.</summary>
public static class RollingCostModelBatchRules
{
    public static RollingCostModelBatchValidator DefaultRollingCostModelBatchValidator()
    {
        return new RollingCostModelBatchValidator(
            new List<IRollingCostModelBatchRule>
            {
                new RollingCostModelBatchRequiredSkillRule("rolling cost model batch needs an entry"),
                new RollingCostModelBatchRequiredTeamRule("rolling cost model batch needs this field"),
                new RollingCostModelBatchTeamLengthRule(255),
                new RollingCostModelBatchSkillLengthRule(120),
                new RollingCostModelBatchRequiredLabelRule("rolling cost model batch needs a value"),
                new RollingCostModelBatchCreatedAtRangeRule(255, 255),
            });
    }
}
