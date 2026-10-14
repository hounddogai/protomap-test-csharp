namespace Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>A rule a garnishment baseline must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentBaselineRule
{
    List<string> Check(GarnishmentBaseline item);
}

public sealed record GarnishmentBaselineStartsAtRangeRule(int Min, int Max) : IGarnishmentBaselineRule
{
    public List<string> Check(GarnishmentBaseline item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentBaselineTimezoneLengthRule(int MaxLength) : IGarnishmentBaselineRule
{
    public List<string> Check(GarnishmentBaseline item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBaselineCodeLengthRule(int MaxLength) : IGarnishmentBaselineRule
{
    public List<string> Check(GarnishmentBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBaselineApprovedGuardRule(double Limit) : IGarnishmentBaselineRule
{
    public List<string> Check(GarnishmentBaseline item)
    {
        if (item.Approved && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBaselineVersionRangeRule(int Min, int Max) : IGarnishmentBaselineRule
{
    public List<string> Check(GarnishmentBaseline item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment baseline.</summary>
public sealed record GarnishmentBaselineValidator(List<IGarnishmentBaselineRule> Rules)
{
    public List<string> Validate(GarnishmentBaseline item)
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

    public void RequireValid(GarnishmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentBaseline> items)
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

/// <summary>Functions of garnishment baseline rules.</summary>
public static class GarnishmentBaselineRules
{
    public static GarnishmentBaselineValidator DefaultGarnishmentBaselineValidator()
    {
        return new GarnishmentBaselineValidator(
            new List<IGarnishmentBaselineRule>
            {
                new GarnishmentBaselineStartsAtRangeRule(80, 86400),
                new GarnishmentBaselineTimezoneLengthRule(3600),
                new GarnishmentBaselineCodeLengthRule(86400),
                new GarnishmentBaselineApprovedGuardRule(8.4),
                new GarnishmentBaselineVersionRangeRule(80, 86400),
            });
    }
}
