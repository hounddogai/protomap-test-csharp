namespace Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>A rule a primary pay run audit must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPayRunAuditRule
{
    List<string> Check(PrimaryPayRunAudit item);
}

public sealed record PrimaryPayRunAuditWindowDaysRangeRule(int Min, int Max) : IPrimaryPayRunAuditRule
{
    public List<string> Check(PrimaryPayRunAudit item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayRunAuditRequiredCodeRule(string Message) : IPrimaryPayRunAuditRule
{
    public List<string> Check(PrimaryPayRunAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayRunAuditChecksumLengthRule(int MaxLength) : IPrimaryPayRunAuditRule
{
    public List<string> Check(PrimaryPayRunAudit item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayRunAuditRequiredCountryRule(string Message) : IPrimaryPayRunAuditRule
{
    public List<string> Check(PrimaryPayRunAudit item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayRunAuditRequiredTitleRule(string Message) : IPrimaryPayRunAuditRule
{
    public List<string> Check(PrimaryPayRunAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary pay run audit.</summary>
public sealed record PrimaryPayRunAuditValidator(List<IPrimaryPayRunAuditRule> Rules)
{
    public List<string> Validate(PrimaryPayRunAudit item)
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

    public void RequireValid(PrimaryPayRunAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPayRunAudit> items)
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

/// <summary>Functions of primary pay run audit rules.</summary>
public static class PrimaryPayRunAuditRules
{
    public static PrimaryPayRunAuditValidator DefaultPrimaryPayRunAuditValidator()
    {
        return new PrimaryPayRunAuditValidator(
            new List<IPrimaryPayRunAuditRule>
            {
                new PrimaryPayRunAuditWindowDaysRangeRule(32, 32),
                new PrimaryPayRunAuditRequiredCodeRule("primary pay run audit needs a value"),
                new PrimaryPayRunAuditChecksumLengthRule(80),
                new PrimaryPayRunAuditRequiredCountryRule("primary pay run audit needs an entry"),
                new PrimaryPayRunAuditRequiredTitleRule("primary pay run audit needs an entry"),
            });
    }
}
