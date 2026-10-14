namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment ledger must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentLedgerRule
{
    List<string> Check(GarnishmentLedger item);
}

public sealed record GarnishmentLedgerRequiredCodeRule(string Message) : IGarnishmentLedgerRule
{
    public List<string> Check(GarnishmentLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentLedgerAllowedCodeRule(List<string> Allowed) : IGarnishmentLedgerRule
{
    public List<string> Check(GarnishmentLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentLedgerTagsLimitRule(int MaxCount, List<string> Banned) : IGarnishmentLedgerRule
{
    public List<string> Check(GarnishmentLedger item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record GarnishmentLedgerNameLengthRule(int MaxLength) : IGarnishmentLedgerRule
{
    public List<string> Check(GarnishmentLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment ledger.</summary>
public sealed record GarnishmentLedgerValidator(List<IGarnishmentLedgerRule> Rules)
{
    public List<string> Validate(GarnishmentLedger item)
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

    public void RequireValid(GarnishmentLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentLedger> items)
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

/// <summary>Functions of garnishment ledger rules.</summary>
public static class GarnishmentLedgerRules
{
    public static GarnishmentLedgerValidator DefaultGarnishmentLedgerValidator()
    {
        return new GarnishmentLedgerValidator(
            new List<IGarnishmentLedgerRule>
            {
                new GarnishmentLedgerRequiredCodeRule("garnishment ledger needs this field"),
                new GarnishmentLedgerAllowedCodeRule(new List<string>()),
                new GarnishmentLedgerTagsLimitRule(1000, new List<string> { "strict" }),
                new GarnishmentLedgerNameLengthRule(64),
            });
    }
}
