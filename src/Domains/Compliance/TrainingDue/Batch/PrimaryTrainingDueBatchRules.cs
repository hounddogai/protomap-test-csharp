namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary training due batch must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTrainingDueBatchRule
{
    List<string> Check(PrimaryTrainingDueBatch item);
}

public sealed record PrimaryTrainingDueBatchCodeLengthRule(int MaxLength) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDueBatchSalaryBandLengthRule(int MaxLength) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDueBatchRequiredSalaryBandRule(string Message) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDueBatchOwnerIdLengthRule(int MaxLength) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDueBatchCodePrefixRule(string Prefix) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDueBatchAllowedNameRule(List<string> Allowed) : IPrimaryTrainingDueBatchRule
{
    public List<string> Check(PrimaryTrainingDueBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary training due batch.</summary>
public sealed record PrimaryTrainingDueBatchValidator(List<IPrimaryTrainingDueBatchRule> Rules)
{
    public List<string> Validate(PrimaryTrainingDueBatch item)
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

    public void RequireValid(PrimaryTrainingDueBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTrainingDueBatch> items)
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

/// <summary>Functions of primary training due batch rules.</summary>
public static class PrimaryTrainingDueBatchRules
{
    public static PrimaryTrainingDueBatchValidator DefaultPrimaryTrainingDueBatchValidator()
    {
        return new PrimaryTrainingDueBatchValidator(
            new List<IPrimaryTrainingDueBatchRule>
            {
                new PrimaryTrainingDueBatchCodeLengthRule(120),
                new PrimaryTrainingDueBatchSalaryBandLengthRule(3600),
                new PrimaryTrainingDueBatchRequiredSalaryBandRule("primary training due batch needs this field"),
                new PrimaryTrainingDueBatchOwnerIdLengthRule(1000),
                new PrimaryTrainingDueBatchCodePrefixRule("ACM"),
                new PrimaryTrainingDueBatchAllowedNameRule(new List<string> { "legacy", "default" }),
            });
    }
}
