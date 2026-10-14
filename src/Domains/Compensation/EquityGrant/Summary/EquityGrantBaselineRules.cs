namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant baseline must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantBaselineRule
{
    List<string> Check(EquityGrantBaseline item);
}

public sealed record EquityGrantBaselineRequiredRegionRule(string Message) : IEquityGrantBaselineRule
{
    public List<string> Check(EquityGrantBaseline item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBaselineLeaveDaysRangeRule(int Min, int Max) : IEquityGrantBaselineRule
{
    public List<string> Check(EquityGrantBaseline item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBaselineAllowedPhoneRule(List<string> Allowed) : IEquityGrantBaselineRule
{
    public List<string> Check(EquityGrantBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBaselineCodePrefixRule(string Prefix) : IEquityGrantBaselineRule
{
    public List<string> Check(EquityGrantBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant baseline.</summary>
public sealed record EquityGrantBaselineValidator(List<IEquityGrantBaselineRule> Rules)
{
    public List<string> Validate(EquityGrantBaseline item)
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

    public void RequireValid(EquityGrantBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantBaseline> items)
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

/// <summary>Functions of equity grant baseline rules.</summary>
public static class EquityGrantBaselineRules
{
    public static EquityGrantBaselineValidator DefaultEquityGrantBaselineValidator()
    {
        return new EquityGrantBaselineValidator(
            new List<IEquityGrantBaselineRule>
            {
                new EquityGrantBaselineRequiredRegionRule("equity grant baseline needs a value"),
                new EquityGrantBaselineLeaveDaysRangeRule(16, 16),
                new EquityGrantBaselineAllowedPhoneRule(new List<string>()),
                new EquityGrantBaselineCodePrefixRule("REF"),
            });
    }
}
