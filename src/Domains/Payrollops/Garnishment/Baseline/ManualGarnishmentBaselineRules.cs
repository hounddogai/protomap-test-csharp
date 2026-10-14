namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual garnishment baseline must pass. Check returns its problems, or none.</summary>
public interface IManualGarnishmentBaselineRule
{
    List<string> Check(ManualGarnishmentBaseline item);
}

public sealed record ManualGarnishmentBaselineRateRangeRule(double Min, double Max) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualGarnishmentBaselineRequiredCodeRule(string Message) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualGarnishmentBaselineMinimumRangeRule(double Min, double Max) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualGarnishmentBaselineCodePrefixRule(string Prefix) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualGarnishmentBaselineFteRangeRule(double Min, double Max) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualGarnishmentBaselineAllowedNameRule(List<string> Allowed) : IManualGarnishmentBaselineRule
{
    public List<string> Check(ManualGarnishmentBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual garnishment baseline.</summary>
public sealed record ManualGarnishmentBaselineValidator(List<IManualGarnishmentBaselineRule> Rules)
{
    public List<string> Validate(ManualGarnishmentBaseline item)
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

    public void RequireValid(ManualGarnishmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualGarnishmentBaseline> items)
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

/// <summary>Functions of manual garnishment baseline rules.</summary>
public static class ManualGarnishmentBaselineRules
{
    public static ManualGarnishmentBaselineValidator DefaultManualGarnishmentBaselineValidator()
    {
        return new ManualGarnishmentBaselineValidator(
            new List<IManualGarnishmentBaselineRule>
            {
                new ManualGarnishmentBaselineRateRangeRule(0.4, 1986.1),
                new ManualGarnishmentBaselineRequiredCodeRule("manual garnishment baseline needs a value"),
                new ManualGarnishmentBaselineMinimumRangeRule(7.6, 1209.4),
                new ManualGarnishmentBaselineCodePrefixRule("X"),
                new ManualGarnishmentBaselineFteRangeRule(3.3, 780.8),
                new ManualGarnishmentBaselineAllowedNameRule(new List<string>()),
            });
    }
}
