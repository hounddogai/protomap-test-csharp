namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>A rule a pending attestation threshold must pass. Check returns its problems, or none.</summary>
public interface IPendingAttestationThresholdRule
{
    List<string> Check(PendingAttestationThreshold item);
}

public sealed record PendingAttestationThresholdRequiredCodeRule(string Message) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationThresholdManagerIdLengthRule(int MaxLength) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationThresholdRequiredManagerIdRule(string Message) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationThresholdDepartmentLengthRule(int MaxLength) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationThresholdQuantityRangeRule(int Min, int Max) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingAttestationThresholdLabelLengthRule(int MaxLength) : IPendingAttestationThresholdRule
{
    public List<string> Check(PendingAttestationThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending attestation threshold.</summary>
public sealed record PendingAttestationThresholdValidator(List<IPendingAttestationThresholdRule> Rules)
{
    public List<string> Validate(PendingAttestationThreshold item)
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

    public void RequireValid(PendingAttestationThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAttestationThreshold> items)
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

/// <summary>Functions of pending attestation threshold rules.</summary>
public static class PendingAttestationThresholdRules
{
    public static PendingAttestationThresholdValidator DefaultPendingAttestationThresholdValidator()
    {
        return new PendingAttestationThresholdValidator(
            new List<IPendingAttestationThresholdRule>
            {
                new PendingAttestationThresholdRequiredCodeRule("pending attestation threshold needs this field"),
                new PendingAttestationThresholdManagerIdLengthRule(120),
                new PendingAttestationThresholdRequiredManagerIdRule("pending attestation threshold needs a value"),
                new PendingAttestationThresholdDepartmentLengthRule(64),
                new PendingAttestationThresholdQuantityRangeRule(16, 86400),
                new PendingAttestationThresholdLabelLengthRule(1000),
            });
    }
}
