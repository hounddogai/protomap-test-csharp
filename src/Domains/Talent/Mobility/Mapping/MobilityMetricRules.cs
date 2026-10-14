namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility metric must pass. Check returns its problems, or none.</summary>
public interface IMobilityMetricRule
{
    List<string> Check(MobilityMetric item);
}

public sealed record MobilityMetricAllowedDepartmentRule(List<string> Allowed) : IMobilityMetricRule
{
    public List<string> Check(MobilityMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is missing" };
        }
        return new List<string>();
    }
}

public sealed record MobilityMetricGradeLengthRule(int MaxLength) : IMobilityMetricRule
{
    public List<string> Check(MobilityMetric item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityMetricRequiredDepartmentRule(string Message) : IMobilityMetricRule
{
    public List<string> Check(MobilityMetric item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityMetricApprovedGuardRule(int Limit) : IMobilityMetricRule
{
    public List<string> Check(MobilityMetric item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility metric.</summary>
public sealed record MobilityMetricValidator(List<IMobilityMetricRule> Rules)
{
    public List<string> Validate(MobilityMetric item)
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

    public void RequireValid(MobilityMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityMetric> items)
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

/// <summary>Functions of mobility metric rules.</summary>
public static class MobilityMetricRules
{
    public static MobilityMetricValidator DefaultMobilityMetricValidator()
    {
        return new MobilityMetricValidator(
            new List<IMobilityMetricRule>
            {
                new MobilityMetricAllowedDepartmentRule(new List<string> { "lenient", "pending", "pending" }),
                new MobilityMetricGradeLengthRule(120),
                new MobilityMetricRequiredDepartmentRule("mobility metric needs an entry"),
                new MobilityMetricApprovedGuardRule(86400),
            });
    }
}
