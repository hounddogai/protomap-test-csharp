namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>A rule a training due digest must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueDigestRule
{
    List<string> Check(TrainingDueDigest item);
}

public sealed record TrainingDueDigestWindowRule(int MaxSpan) : ITrainingDueDigestRule
{
    public List<string> Check(TrainingDueDigest item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueDigestRequiredLabelRule(string Message) : ITrainingDueDigestRule
{
    public List<string> Check(TrainingDueDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueDigestEndsAtRangeRule(int Min, int Max) : ITrainingDueDigestRule
{
    public List<string> Check(TrainingDueDigest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueDigestCodeLengthRule(int MaxLength) : ITrainingDueDigestRule
{
    public List<string> Check(TrainingDueDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueDigestRequiredManagerIdRule(string Message) : ITrainingDueDigestRule
{
    public List<string> Check(TrainingDueDigest item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due digest.</summary>
public sealed record TrainingDueDigestValidator(List<ITrainingDueDigestRule> Rules)
{
    public List<string> Validate(TrainingDueDigest item)
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

    public void RequireValid(TrainingDueDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueDigest> items)
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

/// <summary>Functions of training due digest rules.</summary>
public static class TrainingDueDigestRules
{
    public static TrainingDueDigestValidator DefaultTrainingDueDigestValidator()
    {
        return new TrainingDueDigestValidator(
            new List<ITrainingDueDigestRule>
            {
                new TrainingDueDigestWindowRule(255),
                new TrainingDueDigestRequiredLabelRule("training due digest needs a value"),
                new TrainingDueDigestEndsAtRangeRule(3600, 3600),
                new TrainingDueDigestCodeLengthRule(255),
                new TrainingDueDigestRequiredManagerIdRule("training due digest needs an entry"),
            });
    }
}
