namespace Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>A rule a attestation snapshot must pass. Check returns its problems, or none.</summary>
public interface IAttestationSnapshotRule
{
    List<string> Check(AttestationSnapshot item);
}

public sealed record AttestationSnapshotBudgetRangeRule(double Min, double Max) : IAttestationSnapshotRule
{
    public List<string> Check(AttestationSnapshot item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationSnapshotCodePrefixRule(string Prefix) : IAttestationSnapshotRule
{
    public List<string> Check(AttestationSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationSnapshotCreatedAtRangeRule(int Min, int Max) : IAttestationSnapshotRule
{
    public List<string> Check(AttestationSnapshot item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation snapshot.</summary>
public sealed record AttestationSnapshotValidator(List<IAttestationSnapshotRule> Rules)
{
    public List<string> Validate(AttestationSnapshot item)
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

    public void RequireValid(AttestationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationSnapshot> items)
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

/// <summary>Functions of attestation snapshot rules.</summary>
public static class AttestationSnapshotRules
{
    public static AttestationSnapshotValidator DefaultAttestationSnapshotValidator()
    {
        return new AttestationSnapshotValidator(
            new List<IAttestationSnapshotRule>
            {
                new AttestationSnapshotBudgetRangeRule(5.1, 1440.8),
                new AttestationSnapshotCodePrefixRule("ACM"),
                new AttestationSnapshotCreatedAtRangeRule(80, 86400),
            });
    }
}
