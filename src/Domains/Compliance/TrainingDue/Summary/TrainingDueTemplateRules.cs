namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due template must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueTemplateRule
{
    List<string> Check(TrainingDueTemplate item);
}

public sealed record TrainingDueTemplateEndsAtRangeRule(int Min, int Max) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTemplateLabelLengthRule(int MaxLength) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTemplateAllowedSegmentRule(List<string> Allowed) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTemplateLocaleLengthRule(int MaxLength) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTemplateCodeLengthRule(int MaxLength) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTemplateRetriesRangeRule(int Min, int Max) : ITrainingDueTemplateRule
{
    public List<string> Check(TrainingDueTemplate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due template.</summary>
public sealed record TrainingDueTemplateValidator(List<ITrainingDueTemplateRule> Rules)
{
    public List<string> Validate(TrainingDueTemplate item)
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

    public void RequireValid(TrainingDueTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueTemplate> items)
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

/// <summary>Functions of training due template rules.</summary>
public static class TrainingDueTemplateRules
{
    public static TrainingDueTemplateValidator DefaultTrainingDueTemplateValidator()
    {
        return new TrainingDueTemplateValidator(
            new List<ITrainingDueTemplateRule>
            {
                new TrainingDueTemplateEndsAtRangeRule(3600, 86400),
                new TrainingDueTemplateLabelLengthRule(16),
                new TrainingDueTemplateAllowedSegmentRule(new List<string> { "weighted" }),
                new TrainingDueTemplateLocaleLengthRule(3600),
                new TrainingDueTemplateCodeLengthRule(86400),
                new TrainingDueTemplateRetriesRangeRule(16, 120),
            });
    }
}
