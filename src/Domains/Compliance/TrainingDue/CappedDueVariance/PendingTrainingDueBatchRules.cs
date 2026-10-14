namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>A rule a pending training due batch must pass. Check returns its problems, or none.</summary>
public interface IPendingTrainingDueBatchRule
{
    List<string> Check(PendingTrainingDueBatch item);
}

public sealed record PendingTrainingDueBatchCodeLengthRule(int MaxLength) : IPendingTrainingDueBatchRule
{
    public List<string> Check(PendingTrainingDueBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueBatchPinnedGuardRule(int Limit) : IPendingTrainingDueBatchRule
{
    public List<string> Check(PendingTrainingDueBatch item)
    {
        if (item.Pinned && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueBatchCodePrefixRule(string Prefix) : IPendingTrainingDueBatchRule
{
    public List<string> Check(PendingTrainingDueBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueBatchPhoneLengthRule(int MaxLength) : IPendingTrainingDueBatchRule
{
    public List<string> Check(PendingTrainingDueBatch item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueBatchLabelLengthRule(int MaxLength) : IPendingTrainingDueBatchRule
{
    public List<string> Check(PendingTrainingDueBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending training due batch.</summary>
public sealed record PendingTrainingDueBatchValidator(List<IPendingTrainingDueBatchRule> Rules)
{
    public List<string> Validate(PendingTrainingDueBatch item)
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

    public void RequireValid(PendingTrainingDueBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTrainingDueBatch> items)
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

/// <summary>Functions of pending training due batch rules.</summary>
public static class PendingTrainingDueBatchRules
{
    public static PendingTrainingDueBatchValidator DefaultPendingTrainingDueBatchValidator()
    {
        return new PendingTrainingDueBatchValidator(
            new List<IPendingTrainingDueBatchRule>
            {
                new PendingTrainingDueBatchCodeLengthRule(32),
                new PendingTrainingDueBatchPinnedGuardRule(80),
                new PendingTrainingDueBatchCodePrefixRule("REF"),
                new PendingTrainingDueBatchPhoneLengthRule(64),
                new PendingTrainingDueBatchLabelLengthRule(255),
            });
    }
}
