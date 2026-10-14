namespace Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>A rule a pending attestation cycle must pass. Check returns its problems, or none.</summary>
public interface IPendingAttestationCycleRule
{
    List<string> Check(PendingAttestationCycle item);
}

public sealed record PendingAttestationCycleGradeLengthRule(int MaxLength) : IPendingAttestationCycleRule
{
    public List<string> Check(PendingAttestationCycle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationCycleNameLengthRule(int MaxLength) : IPendingAttestationCycleRule
{
    public List<string> Check(PendingAttestationCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationCycleRequiredNameRule(string Message) : IPendingAttestationCycleRule
{
    public List<string> Check(PendingAttestationCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending attestation cycle.</summary>
public sealed record PendingAttestationCycleValidator(List<IPendingAttestationCycleRule> Rules)
{
    public List<string> Validate(PendingAttestationCycle item)
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

    public void RequireValid(PendingAttestationCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAttestationCycle> items)
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

/// <summary>Functions of pending attestation cycle rules.</summary>
public static class PendingAttestationCycleRules
{
    public static PendingAttestationCycleValidator DefaultPendingAttestationCycleValidator()
    {
        return new PendingAttestationCycleValidator(
            new List<IPendingAttestationCycleRule>
            {
                new PendingAttestationCycleGradeLengthRule(1000),
                new PendingAttestationCycleNameLengthRule(255),
                new PendingAttestationCycleRequiredNameRule("pending attestation cycle needs an entry"),
            });
    }
}
