namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount audit must pass. Check returns its problems, or none.</summary>
public interface IHeadcountAuditRule
{
    List<string> Check(HeadcountAudit item);
}

public sealed record HeadcountAuditRequiredCodeRule(string Message) : IHeadcountAuditRule
{
    public List<string> Check(HeadcountAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAuditAllowedCountryRule(List<string> Allowed) : IHeadcountAuditRule
{
    public List<string> Check(HeadcountAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAuditAmountRangeRule(double Min, double Max) : IHeadcountAuditRule
{
    public List<string> Check(HeadcountAudit item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount audit.</summary>
public sealed record HeadcountAuditValidator(List<IHeadcountAuditRule> Rules)
{
    public List<string> Validate(HeadcountAudit item)
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

    public void RequireValid(HeadcountAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountAudit> items)
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

/// <summary>Functions of headcount audit rules.</summary>
public static class HeadcountAuditRules
{
    public static HeadcountAuditValidator DefaultHeadcountAuditValidator()
    {
        return new HeadcountAuditValidator(
            new List<IHeadcountAuditRule>
            {
                new HeadcountAuditRequiredCodeRule("headcount audit needs this field"),
                new HeadcountAuditAllowedCountryRule(new List<string> { "legacy", "capped" }),
                new HeadcountAuditAmountRangeRule(6.7, 3451.1),
            });
    }
}
