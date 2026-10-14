namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction checkpoint must pass. Check returns its problems, or none.</summary>
public interface IDeductionCheckpointRule
{
    List<string> Check(DeductionCheckpoint item);
}

public sealed record DeductionCheckpointRequiredCodeRule(string Message) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionCheckpointAllowedNameRule(List<string> Allowed) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DeductionCheckpointUpdatedAtRangeRule(int Min, int Max) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionCheckpointCodeLengthRule(int MaxLength) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionCheckpointApprovedGuardRule(double Limit) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        if (item.Approved && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record DeductionCheckpointRequiredNameRule(string Message) : IDeductionCheckpointRule
{
    public List<string> Check(DeductionCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction checkpoint.</summary>
public sealed record DeductionCheckpointValidator(List<IDeductionCheckpointRule> Rules)
{
    public List<string> Validate(DeductionCheckpoint item)
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

    public void RequireValid(DeductionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionCheckpoint> items)
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

/// <summary>Functions of deduction checkpoint rules.</summary>
public static class DeductionCheckpointRules
{
    public static DeductionCheckpointValidator DefaultDeductionCheckpointValidator()
    {
        return new DeductionCheckpointValidator(
            new List<IDeductionCheckpointRule>
            {
                new DeductionCheckpointRequiredCodeRule("deduction checkpoint needs an entry"),
                new DeductionCheckpointAllowedNameRule(new List<string> { "manual", "regional", "default" }),
                new DeductionCheckpointUpdatedAtRangeRule(32, 64),
                new DeductionCheckpointCodeLengthRule(86400),
                new DeductionCheckpointApprovedGuardRule(7.0),
                new DeductionCheckpointRequiredNameRule("deduction checkpoint needs an entry"),
            });
    }
}
