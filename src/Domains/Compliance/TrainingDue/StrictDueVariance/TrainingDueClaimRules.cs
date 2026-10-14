namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>A rule a training due claim must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueClaimRule
{
    List<string> Check(TrainingDueClaim item);
}

public sealed record TrainingDueClaimTitleLengthRule(int MaxLength) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueClaimRequiredTitleRule(string Message) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueClaimRequiredCodeRule(string Message) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueClaimVisibleGuardRule(int Limit) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        if (item.Visible && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueClaimExternalRefLengthRule(int MaxLength) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueClaimCodeLengthRule(int MaxLength) : ITrainingDueClaimRule
{
    public List<string> Check(TrainingDueClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due claim.</summary>
public sealed record TrainingDueClaimValidator(List<ITrainingDueClaimRule> Rules)
{
    public List<string> Validate(TrainingDueClaim item)
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

    public void RequireValid(TrainingDueClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueClaim> items)
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

/// <summary>Functions of training due claim rules.</summary>
public static class TrainingDueClaimRules
{
    public static TrainingDueClaimValidator DefaultTrainingDueClaimValidator()
    {
        return new TrainingDueClaimValidator(
            new List<ITrainingDueClaimRule>
            {
                new TrainingDueClaimTitleLengthRule(120),
                new TrainingDueClaimRequiredTitleRule("training due claim needs an entry"),
                new TrainingDueClaimRequiredCodeRule("training due claim needs this field"),
                new TrainingDueClaimVisibleGuardRule(120),
                new TrainingDueClaimExternalRefLengthRule(80),
                new TrainingDueClaimCodeLengthRule(120),
            });
    }
}
