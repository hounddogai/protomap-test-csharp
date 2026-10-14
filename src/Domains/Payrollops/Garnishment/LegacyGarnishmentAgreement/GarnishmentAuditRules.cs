namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment audit must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentAuditRule
{
    List<string> Check(GarnishmentAudit item);
}

public sealed record GarnishmentAuditAllowedCodeRule(List<string> Allowed) : IGarnishmentAuditRule
{
    public List<string> Check(GarnishmentAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAuditLabelLengthRule(int MaxLength) : IGarnishmentAuditRule
{
    public List<string> Check(GarnishmentAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAuditLeaveDaysRangeRule(int Min, int Max) : IGarnishmentAuditRule
{
    public List<string> Check(GarnishmentAudit item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAuditCodePrefixRule(string Prefix) : IGarnishmentAuditRule
{
    public List<string> Check(GarnishmentAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAuditRequiredNameRule(string Message) : IGarnishmentAuditRule
{
    public List<string> Check(GarnishmentAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment audit.</summary>
public sealed record GarnishmentAuditValidator(List<IGarnishmentAuditRule> Rules)
{
    public List<string> Validate(GarnishmentAudit item)
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

    public void RequireValid(GarnishmentAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentAudit> items)
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

/// <summary>Functions of garnishment audit rules.</summary>
public static class GarnishmentAuditRules
{
    public static GarnishmentAuditValidator DefaultGarnishmentAuditValidator()
    {
        return new GarnishmentAuditValidator(
            new List<IGarnishmentAuditRule>
            {
                new GarnishmentAuditAllowedCodeRule(new List<string>()),
                new GarnishmentAuditLabelLengthRule(32),
                new GarnishmentAuditLeaveDaysRangeRule(32, 3600),
                new GarnishmentAuditCodePrefixRule("X"),
                new GarnishmentAuditRequiredNameRule("garnishment audit needs an entry"),
            });
    }
}
