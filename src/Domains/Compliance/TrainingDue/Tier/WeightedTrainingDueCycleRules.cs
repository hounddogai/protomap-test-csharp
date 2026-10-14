namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted training due cycle must pass. Check returns its problems, or none.</summary>
public interface IWeightedTrainingDueCycleRule
{
    List<string> Check(WeightedTrainingDueCycle item);
}

public sealed record WeightedTrainingDueCycleLabelLengthRule(int MaxLength) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueCycleManagerIdLengthRule(int MaxLength) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueCycleRequiredSkillRule(string Message) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueCycleUpdatedAtRangeRule(int Min, int Max) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
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

public sealed record WeightedTrainingDueCycleAllowedLabelRule(List<string> Allowed) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueCycleCodeLengthRule(int MaxLength) : IWeightedTrainingDueCycleRule
{
    public List<string> Check(WeightedTrainingDueCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted training due cycle.</summary>
public sealed record WeightedTrainingDueCycleValidator(List<IWeightedTrainingDueCycleRule> Rules)
{
    public List<string> Validate(WeightedTrainingDueCycle item)
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

    public void RequireValid(WeightedTrainingDueCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTrainingDueCycle> items)
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

/// <summary>Functions of weighted training due cycle rules.</summary>
public static class WeightedTrainingDueCycleRules
{
    public static WeightedTrainingDueCycleValidator DefaultWeightedTrainingDueCycleValidator()
    {
        return new WeightedTrainingDueCycleValidator(
            new List<IWeightedTrainingDueCycleRule>
            {
                new WeightedTrainingDueCycleLabelLengthRule(64),
                new WeightedTrainingDueCycleManagerIdLengthRule(120),
                new WeightedTrainingDueCycleRequiredSkillRule("weighted training due cycle needs a value"),
                new WeightedTrainingDueCycleUpdatedAtRangeRule(80, 80),
                new WeightedTrainingDueCycleAllowedLabelRule(new List<string> { "manual" }),
                new WeightedTrainingDueCycleCodeLengthRule(86400),
            });
    }
}
