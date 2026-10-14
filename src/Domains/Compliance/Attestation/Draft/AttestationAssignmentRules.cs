namespace Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>A rule a attestation assignment must pass. Check returns its problems, or none.</summary>
public interface IAttestationAssignmentRule
{
    List<string> Check(AttestationAssignment item);
}

public sealed record AttestationAssignmentRequiredNotesRule(string Message) : IAttestationAssignmentRule
{
    public List<string> Check(AttestationAssignment item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationAssignmentAttemptsRangeRule(int Min, int Max) : IAttestationAssignmentRule
{
    public List<string> Check(AttestationAssignment item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationAssignmentSequenceRangeRule(int Min, int Max) : IAttestationAssignmentRule
{
    public List<string> Check(AttestationAssignment item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAssignmentRequiredCodeRule(string Message) : IAttestationAssignmentRule
{
    public List<string> Check(AttestationAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation assignment.</summary>
public sealed record AttestationAssignmentValidator(List<IAttestationAssignmentRule> Rules)
{
    public List<string> Validate(AttestationAssignment item)
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

    public void RequireValid(AttestationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationAssignment> items)
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

/// <summary>Functions of attestation assignment rules.</summary>
public static class AttestationAssignmentRules
{
    public static AttestationAssignmentValidator DefaultAttestationAssignmentValidator()
    {
        return new AttestationAssignmentValidator(
            new List<IAttestationAssignmentRule>
            {
                new AttestationAssignmentRequiredNotesRule("attestation assignment needs an entry"),
                new AttestationAssignmentAttemptsRangeRule(32, 32),
                new AttestationAssignmentSequenceRangeRule(64, 3600),
                new AttestationAssignmentRequiredCodeRule("attestation assignment needs this field"),
            });
    }
}
