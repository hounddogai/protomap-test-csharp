namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>A rule a scheduled training due variance must pass. Check returns its problems, or none.</summary>
public interface IScheduledTrainingDueVarianceRule
{
    List<string> Check(ScheduledTrainingDueVariance item);
}

public sealed record ScheduledTrainingDueVarianceExternalRefLengthRule(int MaxLength) : IScheduledTrainingDueVarianceRule
{
    public List<string> Check(ScheduledTrainingDueVariance item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueVarianceRequiredNameRule(string Message) : IScheduledTrainingDueVarianceRule
{
    public List<string> Check(ScheduledTrainingDueVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueVarianceCodeLengthRule(int MaxLength) : IScheduledTrainingDueVarianceRule
{
    public List<string> Check(ScheduledTrainingDueVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueVarianceCodePrefixRule(string Prefix) : IScheduledTrainingDueVarianceRule
{
    public List<string> Check(ScheduledTrainingDueVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled training due variance.</summary>
public sealed record ScheduledTrainingDueVarianceValidator(List<IScheduledTrainingDueVarianceRule> Rules)
{
    public List<string> Validate(ScheduledTrainingDueVariance item)
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

    public void RequireValid(ScheduledTrainingDueVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTrainingDueVariance> items)
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

/// <summary>Functions of scheduled training due variance rules.</summary>
public static class ScheduledTrainingDueVarianceRules
{
    public static ScheduledTrainingDueVarianceValidator DefaultScheduledTrainingDueVarianceValidator()
    {
        return new ScheduledTrainingDueVarianceValidator(
            new List<IScheduledTrainingDueVarianceRule>
            {
                new ScheduledTrainingDueVarianceExternalRefLengthRule(255),
                new ScheduledTrainingDueVarianceRequiredNameRule("scheduled training due variance needs this field"),
                new ScheduledTrainingDueVarianceCodeLengthRule(255),
                new ScheduledTrainingDueVarianceCodePrefixRule("OPS"),
            });
    }
}
