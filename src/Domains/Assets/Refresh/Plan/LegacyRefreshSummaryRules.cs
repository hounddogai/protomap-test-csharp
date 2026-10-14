namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy refresh summary must pass. Check returns its problems, or none.</summary>
public interface ILegacyRefreshSummaryRule
{
    List<string> Check(LegacyRefreshSummary item);
}

public sealed record LegacyRefreshSummaryCodePrefixRule(string Prefix) : ILegacyRefreshSummaryRule
{
    public List<string> Check(LegacyRefreshSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRefreshSummaryExternalRefLengthRule(int MaxLength) : ILegacyRefreshSummaryRule
{
    public List<string> Check(LegacyRefreshSummary item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRefreshSummaryAllowedLabelRule(List<string> Allowed) : ILegacyRefreshSummaryRule
{
    public List<string> Check(LegacyRefreshSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy refresh summary.</summary>
public sealed record LegacyRefreshSummaryValidator(List<ILegacyRefreshSummaryRule> Rules)
{
    public List<string> Validate(LegacyRefreshSummary item)
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

    public void RequireValid(LegacyRefreshSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRefreshSummary> items)
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

/// <summary>Functions of legacy refresh summary rules.</summary>
public static class LegacyRefreshSummaryRules
{
    public static LegacyRefreshSummaryValidator DefaultLegacyRefreshSummaryValidator()
    {
        return new LegacyRefreshSummaryValidator(
            new List<ILegacyRefreshSummaryRule>
            {
                new LegacyRefreshSummaryCodePrefixRule("REF"),
                new LegacyRefreshSummaryExternalRefLengthRule(1000),
                new LegacyRefreshSummaryAllowedLabelRule(new List<string> { "rolling" }),
            });
    }
}
