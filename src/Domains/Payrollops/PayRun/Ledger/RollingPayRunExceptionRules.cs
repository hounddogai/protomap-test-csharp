namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pay run exception must pass. Check returns its problems, or none.</summary>
public interface IRollingPayRunExceptionRule
{
    List<string> Check(RollingPayRunException item);
}

public sealed record RollingPayRunExceptionCodePrefixRule(string Prefix) : IRollingPayRunExceptionRule
{
    public List<string> Check(RollingPayRunException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunExceptionNotesLengthRule(int MaxLength) : IRollingPayRunExceptionRule
{
    public List<string> Check(RollingPayRunException item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunExceptionRequiredDepartmentRule(string Message) : IRollingPayRunExceptionRule
{
    public List<string> Check(RollingPayRunException item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunExceptionAllowedDepartmentRule(List<string> Allowed) : IRollingPayRunExceptionRule
{
    public List<string> Check(RollingPayRunException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunExceptionNameLengthRule(int MaxLength) : IRollingPayRunExceptionRule
{
    public List<string> Check(RollingPayRunException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pay run exception.</summary>
public sealed record RollingPayRunExceptionValidator(List<IRollingPayRunExceptionRule> Rules)
{
    public List<string> Validate(RollingPayRunException item)
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

    public void RequireValid(RollingPayRunException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPayRunException> items)
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

/// <summary>Functions of rolling pay run exception rules.</summary>
public static class RollingPayRunExceptionRules
{
    public static RollingPayRunExceptionValidator DefaultRollingPayRunExceptionValidator()
    {
        return new RollingPayRunExceptionValidator(
            new List<IRollingPayRunExceptionRule>
            {
                new RollingPayRunExceptionCodePrefixRule("X"),
                new RollingPayRunExceptionNotesLengthRule(1000),
                new RollingPayRunExceptionRequiredDepartmentRule("rolling pay run exception needs this field"),
                new RollingPayRunExceptionAllowedDepartmentRule(new List<string> { "manual" }),
                new RollingPayRunExceptionNameLengthRule(1000),
            });
    }
}
