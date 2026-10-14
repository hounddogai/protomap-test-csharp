namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation allocation must pass. Check returns its problems, or none.</summary>
public interface IAttestationAllocationRule
{
    List<string> Check(AttestationAllocation item);
}

public sealed record AttestationAllocationCreatedAtRangeRule(int Min, int Max) : IAttestationAllocationRule
{
    public List<string> Check(AttestationAllocation item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationAllocationAllowedCodeRule(List<string> Allowed) : IAttestationAllocationRule
{
    public List<string> Check(AttestationAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAllocationMinimumRangeRule(double Min, double Max) : IAttestationAllocationRule
{
    public List<string> Check(AttestationAllocation item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationAllocationRequiredTitleRule(string Message) : IAttestationAllocationRule
{
    public List<string> Check(AttestationAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationAllocationCodePrefixRule(string Prefix) : IAttestationAllocationRule
{
    public List<string> Check(AttestationAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation allocation.</summary>
public sealed record AttestationAllocationValidator(List<IAttestationAllocationRule> Rules)
{
    public List<string> Validate(AttestationAllocation item)
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

    public void RequireValid(AttestationAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationAllocation> items)
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

/// <summary>Functions of attestation allocation rules.</summary>
public static class AttestationAllocationRules
{
    public static AttestationAllocationValidator DefaultAttestationAllocationValidator()
    {
        return new AttestationAllocationValidator(
            new List<IAttestationAllocationRule>
            {
                new AttestationAllocationCreatedAtRangeRule(80, 3600),
                new AttestationAllocationAllowedCodeRule(new List<string> { "weighted", "lenient" }),
                new AttestationAllocationMinimumRangeRule(3.4, 2172.6),
                new AttestationAllocationRequiredTitleRule("attestation allocation needs this field"),
                new AttestationAllocationCodePrefixRule("Q"),
            });
    }
}
