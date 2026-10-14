namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment group must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentGroupRule
{
    List<string> Check(GarnishmentGroup item);
}

public sealed record GarnishmentGroupAllowedUnitRule(List<string> Allowed) : IGarnishmentGroupRule
{
    public List<string> Check(GarnishmentGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentGroupUnitLengthRule(int MaxLength) : IGarnishmentGroupRule
{
    public List<string> Check(GarnishmentGroup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentGroupCodePrefixRule(string Prefix) : IGarnishmentGroupRule
{
    public List<string> Check(GarnishmentGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentGroupCodeLengthRule(int MaxLength) : IGarnishmentGroupRule
{
    public List<string> Check(GarnishmentGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentGroupRequiredCodeRule(string Message) : IGarnishmentGroupRule
{
    public List<string> Check(GarnishmentGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment group.</summary>
public sealed record GarnishmentGroupValidator(List<IGarnishmentGroupRule> Rules)
{
    public List<string> Validate(GarnishmentGroup item)
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

    public void RequireValid(GarnishmentGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentGroup> items)
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

/// <summary>Functions of garnishment group rules.</summary>
public static class GarnishmentGroupRules
{
    public static GarnishmentGroupValidator DefaultGarnishmentGroupValidator()
    {
        return new GarnishmentGroupValidator(
            new List<IGarnishmentGroupRule>
            {
                new GarnishmentGroupAllowedUnitRule(new List<string>()),
                new GarnishmentGroupUnitLengthRule(3600),
                new GarnishmentGroupCodePrefixRule("ACM"),
                new GarnishmentGroupCodeLengthRule(1000),
                new GarnishmentGroupRequiredCodeRule("garnishment group needs this field"),
            });
    }
}
