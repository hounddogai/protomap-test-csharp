namespace Acme.Workforce.Absence.Policy.Assignment;

/// <summary>A rule a policy entry must pass. Check returns its problems, or none.</summary>
public interface IPolicyEntryRule
{
    List<string> Check(PolicyEntry item);
}

public sealed record PolicyEntryNameLengthRule(int MaxLength) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEntryRequiredCodeRule(string Message) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyEntryArchivedGuardRule(double Limit) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        if (item.Archived && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEntryRequiredLabelRule(string Message) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyEntryCodeLengthRule(int MaxLength) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyEntryCodePrefixRule(string Prefix) : IPolicyEntryRule
{
    public List<string> Check(PolicyEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy entry.</summary>
public sealed record PolicyEntryValidator(List<IPolicyEntryRule> Rules)
{
    public List<string> Validate(PolicyEntry item)
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

    public void RequireValid(PolicyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyEntry> items)
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

/// <summary>Functions of policy entry rules.</summary>
public static class PolicyEntryRules
{
    public static PolicyEntryValidator DefaultPolicyEntryValidator()
    {
        return new PolicyEntryValidator(
            new List<IPolicyEntryRule>
            {
                new PolicyEntryNameLengthRule(255),
                new PolicyEntryRequiredCodeRule("policy entry needs an entry"),
                new PolicyEntryArchivedGuardRule(3.9),
                new PolicyEntryRequiredLabelRule("policy entry needs a value"),
                new PolicyEntryCodeLengthRule(80),
                new PolicyEntryCodePrefixRule("OPS"),
            });
    }
}
