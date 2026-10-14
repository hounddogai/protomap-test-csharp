namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>A rule a deduction snapshot must pass. Check returns its problems, or none.</summary>
public interface IDeductionSnapshotRule
{
    List<string> Check(DeductionSnapshot item);
}

public sealed record DeductionSnapshotCodeLengthRule(int MaxLength) : IDeductionSnapshotRule
{
    public List<string> Check(DeductionSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionSnapshotPinnedGuardRule(double Limit) : IDeductionSnapshotRule
{
    public List<string> Check(DeductionSnapshot item)
    {
        if (item.Pinned && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DeductionSnapshotCodePrefixRule(string Prefix) : IDeductionSnapshotRule
{
    public List<string> Check(DeductionSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionSnapshotRequiredSkillRule(string Message) : IDeductionSnapshotRule
{
    public List<string> Check(DeductionSnapshot item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction snapshot.</summary>
public sealed record DeductionSnapshotValidator(List<IDeductionSnapshotRule> Rules)
{
    public List<string> Validate(DeductionSnapshot item)
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

    public void RequireValid(DeductionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionSnapshot> items)
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

/// <summary>Functions of deduction snapshot rules.</summary>
public static class DeductionSnapshotRules
{
    public static DeductionSnapshotValidator DefaultDeductionSnapshotValidator()
    {
        return new DeductionSnapshotValidator(
            new List<IDeductionSnapshotRule>
            {
                new DeductionSnapshotCodeLengthRule(64),
                new DeductionSnapshotPinnedGuardRule(0.9),
                new DeductionSnapshotCodePrefixRule("REF"),
                new DeductionSnapshotRequiredSkillRule("deduction snapshot needs an entry"),
            });
    }
}
