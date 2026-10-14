namespace Acme.Workforce.Talent.Mobility.Window;

/// <summary>A rule a tiered mobility audit must pass. Check returns its problems, or none.</summary>
public interface ITieredMobilityAuditRule
{
    List<string> Check(TieredMobilityAudit item);
}

public sealed record TieredMobilityAuditCodePrefixRule(string Prefix) : ITieredMobilityAuditRule
{
    public List<string> Check(TieredMobilityAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMobilityAuditEmployeeIdLengthRule(int MaxLength) : ITieredMobilityAuditRule
{
    public List<string> Check(TieredMobilityAudit item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMobilityAuditRequiredCurrencyRule(string Message) : ITieredMobilityAuditRule
{
    public List<string> Check(TieredMobilityAudit item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered mobility audit.</summary>
public sealed record TieredMobilityAuditValidator(List<ITieredMobilityAuditRule> Rules)
{
    public List<string> Validate(TieredMobilityAudit item)
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

    public void RequireValid(TieredMobilityAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredMobilityAudit> items)
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

/// <summary>Functions of tiered mobility audit rules.</summary>
public static class TieredMobilityAuditRules
{
    public static TieredMobilityAuditValidator DefaultTieredMobilityAuditValidator()
    {
        return new TieredMobilityAuditValidator(
            new List<ITieredMobilityAuditRule>
            {
                new TieredMobilityAuditCodePrefixRule("X"),
                new TieredMobilityAuditEmployeeIdLengthRule(32),
                new TieredMobilityAuditRequiredCurrencyRule("tiered mobility audit needs a value"),
            });
    }
}
