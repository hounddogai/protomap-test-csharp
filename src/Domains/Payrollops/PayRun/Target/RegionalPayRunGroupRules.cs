namespace Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>A rule a regional pay run group must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayRunGroupRule
{
    List<string> Check(RegionalPayRunGroup item);
}

public sealed record RegionalPayRunGroupRequiredCodeRule(string Message) : IRegionalPayRunGroupRule
{
    public List<string> Check(RegionalPayRunGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunGroupRequiredSummaryRule(string Message) : IRegionalPayRunGroupRule
{
    public List<string> Check(RegionalPayRunGroup item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunGroupSummaryLengthRule(int MaxLength) : IRegionalPayRunGroupRule
{
    public List<string> Check(RegionalPayRunGroup item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pay run group.</summary>
public sealed record RegionalPayRunGroupValidator(List<IRegionalPayRunGroupRule> Rules)
{
    public List<string> Validate(RegionalPayRunGroup item)
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

    public void RequireValid(RegionalPayRunGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayRunGroup> items)
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

/// <summary>Functions of regional pay run group rules.</summary>
public static class RegionalPayRunGroupRules
{
    public static RegionalPayRunGroupValidator DefaultRegionalPayRunGroupValidator()
    {
        return new RegionalPayRunGroupValidator(
            new List<IRegionalPayRunGroupRule>
            {
                new RegionalPayRunGroupRequiredCodeRule("regional pay run group needs this field"),
                new RegionalPayRunGroupRequiredSummaryRule("regional pay run group needs an entry"),
                new RegionalPayRunGroupSummaryLengthRule(86400),
            });
    }
}
