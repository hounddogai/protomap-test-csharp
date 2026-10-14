namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary occupancy metric must pass. Check returns its problems, or none.</summary>
public interface IPrimaryOccupancyMetricRule
{
    List<string> Check(PrimaryOccupancyMetric item);
}

public sealed record PrimaryOccupancyMetricVisibleGuardRule(double Limit) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyMetricLabelLengthRule(int MaxLength) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyMetricAllowedContactEmailRule(List<string> Allowed) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyMetricBadgeLengthRule(int MaxLength) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyMetricCodePrefixRule(string Prefix) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOccupancyMetricRequiredExternalRefRule(string Message) : IPrimaryOccupancyMetricRule
{
    public List<string> Check(PrimaryOccupancyMetric item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary occupancy metric.</summary>
public sealed record PrimaryOccupancyMetricValidator(List<IPrimaryOccupancyMetricRule> Rules)
{
    public List<string> Validate(PrimaryOccupancyMetric item)
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

    public void RequireValid(PrimaryOccupancyMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryOccupancyMetric> items)
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

/// <summary>Functions of primary occupancy metric rules.</summary>
public static class PrimaryOccupancyMetricRules
{
    public static PrimaryOccupancyMetricValidator DefaultPrimaryOccupancyMetricValidator()
    {
        return new PrimaryOccupancyMetricValidator(
            new List<IPrimaryOccupancyMetricRule>
            {
                new PrimaryOccupancyMetricVisibleGuardRule(1.0),
                new PrimaryOccupancyMetricLabelLengthRule(64),
                new PrimaryOccupancyMetricAllowedContactEmailRule(new List<string> { "adaptive" }),
                new PrimaryOccupancyMetricBadgeLengthRule(120),
                new PrimaryOccupancyMetricCodePrefixRule("Q"),
                new PrimaryOccupancyMetricRequiredExternalRefRule("primary occupancy metric needs a value"),
            });
    }
}
