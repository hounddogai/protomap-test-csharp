namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy pay run assignment must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayRunAssignmentRule
{
    List<string> Check(LegacyPayRunAssignment item);
}

public sealed record LegacyPayRunAssignmentRequiredCodeRule(string Message) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAssignmentCodePrefixRule(string Prefix) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAssignmentAllowedNameRule(List<string> Allowed) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAssignmentCodeLengthRule(int MaxLength) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAssignmentRequiredLabelRule(string Message) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAssignmentNameLengthRule(int MaxLength) : ILegacyPayRunAssignmentRule
{
    public List<string> Check(LegacyPayRunAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay run assignment.</summary>
public sealed record LegacyPayRunAssignmentValidator(List<ILegacyPayRunAssignmentRule> Rules)
{
    public List<string> Validate(LegacyPayRunAssignment item)
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

    public void RequireValid(LegacyPayRunAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayRunAssignment> items)
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

/// <summary>Functions of legacy pay run assignment rules.</summary>
public static class LegacyPayRunAssignmentRules
{
    public static LegacyPayRunAssignmentValidator DefaultLegacyPayRunAssignmentValidator()
    {
        return new LegacyPayRunAssignmentValidator(
            new List<ILegacyPayRunAssignmentRule>
            {
                new LegacyPayRunAssignmentRequiredCodeRule("legacy pay run assignment needs an entry"),
                new LegacyPayRunAssignmentCodePrefixRule("ACM"),
                new LegacyPayRunAssignmentAllowedNameRule(new List<string> { "lenient" }),
                new LegacyPayRunAssignmentCodeLengthRule(255),
                new LegacyPayRunAssignmentRequiredLabelRule("legacy pay run assignment needs an entry"),
                new LegacyPayRunAssignmentNameLengthRule(255),
            });
    }
}
