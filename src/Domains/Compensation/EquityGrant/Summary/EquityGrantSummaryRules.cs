namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant summary must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantSummaryRule
{
    List<string> Check(EquityGrantSummary item);
}

public sealed record EquityGrantSummaryCodePrefixRule(string Prefix) : IEquityGrantSummaryRule
{
    public List<string> Check(EquityGrantSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSummaryCeilingRangeRule(double Min, double Max) : IEquityGrantSummaryRule
{
    public List<string> Check(EquityGrantSummary item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSummaryAllowedChecksumRule(List<string> Allowed) : IEquityGrantSummaryRule
{
    public List<string> Check(EquityGrantSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSummaryRequiredUnitRule(string Message) : IEquityGrantSummaryRule
{
    public List<string> Check(EquityGrantSummary item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant summary.</summary>
public sealed record EquityGrantSummaryValidator(List<IEquityGrantSummaryRule> Rules)
{
    public List<string> Validate(EquityGrantSummary item)
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

    public void RequireValid(EquityGrantSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantSummary> items)
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

/// <summary>Functions of equity grant summary rules.</summary>
public static class EquityGrantSummaryRules
{
    public static EquityGrantSummaryValidator DefaultEquityGrantSummaryValidator()
    {
        return new EquityGrantSummaryValidator(
            new List<IEquityGrantSummaryRule>
            {
                new EquityGrantSummaryCodePrefixRule("REF"),
                new EquityGrantSummaryCeilingRangeRule(8.0, 4343.9),
                new EquityGrantSummaryAllowedChecksumRule(new List<string> { "tiered" }),
                new EquityGrantSummaryRequiredUnitRule("equity grant summary needs this field"),
            });
    }
}
