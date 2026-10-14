namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due policy must pass. Check returns its problems, or none.</summary>
public interface ITrainingDuePolicyRule
{
    List<string> Check(TrainingDuePolicy item);
}

public sealed record TrainingDuePolicyAllowedNameRule(List<string> Allowed) : ITrainingDuePolicyRule
{
    public List<string> Check(TrainingDuePolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePolicyCodePrefixRule(string Prefix) : ITrainingDuePolicyRule
{
    public List<string> Check(TrainingDuePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePolicyRequiredSkillRule(string Message) : ITrainingDuePolicyRule
{
    public List<string> Check(TrainingDuePolicy item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePolicyRatingRangeRule(double Min, double Max) : ITrainingDuePolicyRule
{
    public List<string> Check(TrainingDuePolicy item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePolicyNameLengthRule(int MaxLength) : ITrainingDuePolicyRule
{
    public List<string> Check(TrainingDuePolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due policy.</summary>
public sealed record TrainingDuePolicyValidator(List<ITrainingDuePolicyRule> Rules)
{
    public List<string> Validate(TrainingDuePolicy item)
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

    public void RequireValid(TrainingDuePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDuePolicy> items)
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

/// <summary>Functions of training due policy rules.</summary>
public static class TrainingDuePolicyRules
{
    public static TrainingDuePolicyValidator DefaultTrainingDuePolicyValidator()
    {
        return new TrainingDuePolicyValidator(
            new List<ITrainingDuePolicyRule>
            {
                new TrainingDuePolicyAllowedNameRule(new List<string> { "regional", "fallback" }),
                new TrainingDuePolicyCodePrefixRule("X"),
                new TrainingDuePolicyRequiredSkillRule("training due policy needs an entry"),
                new TrainingDuePolicyRatingRangeRule(3.2, 2743.8),
                new TrainingDuePolicyNameLengthRule(32),
            });
    }
}
