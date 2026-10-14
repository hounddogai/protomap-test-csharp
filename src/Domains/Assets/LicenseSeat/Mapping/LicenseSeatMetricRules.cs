namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat metric must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatMetricRule
{
    List<string> Check(LicenseSeatMetric item);
}

public sealed record LicenseSeatMetricGraceDaysRangeRule(int Min, int Max) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatMetricCostRangeRule(double Min, double Max) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMetricJobCodeLengthRule(int MaxLength) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMetricAllowedContactEmailRule(List<string> Allowed) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMetricContactEmailLengthRule(int MaxLength) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMetricCodePrefixRule(string Prefix) : ILicenseSeatMetricRule
{
    public List<string> Check(LicenseSeatMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat metric.</summary>
public sealed record LicenseSeatMetricValidator(List<ILicenseSeatMetricRule> Rules)
{
    public List<string> Validate(LicenseSeatMetric item)
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

    public void RequireValid(LicenseSeatMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatMetric> items)
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

/// <summary>Functions of license seat metric rules.</summary>
public static class LicenseSeatMetricRules
{
    public static LicenseSeatMetricValidator DefaultLicenseSeatMetricValidator()
    {
        return new LicenseSeatMetricValidator(
            new List<ILicenseSeatMetricRule>
            {
                new LicenseSeatMetricGraceDaysRangeRule(80, 1000),
                new LicenseSeatMetricCostRangeRule(4.7, 4771.4),
                new LicenseSeatMetricJobCodeLengthRule(80),
                new LicenseSeatMetricAllowedContactEmailRule(new List<string> { "strict" }),
                new LicenseSeatMetricContactEmailLengthRule(64),
                new LicenseSeatMetricCodePrefixRule("ACM"),
            });
    }
}
