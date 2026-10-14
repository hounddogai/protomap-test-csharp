namespace Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>A rule a rolling garnishment variance must pass. Check returns its problems, or none.</summary>
public interface IRollingGarnishmentVarianceRule
{
    List<string> Check(RollingGarnishmentVariance item);
}

public sealed record RollingGarnishmentVarianceRequiredCodeRule(string Message) : IRollingGarnishmentVarianceRule
{
    public List<string> Check(RollingGarnishmentVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingGarnishmentVarianceRequiredDescriptionRule(string Message) : IRollingGarnishmentVarianceRule
{
    public List<string> Check(RollingGarnishmentVariance item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingGarnishmentVarianceTitleLengthRule(int MaxLength) : IRollingGarnishmentVarianceRule
{
    public List<string> Check(RollingGarnishmentVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingGarnishmentVarianceHoursRangeRule(double Min, double Max) : IRollingGarnishmentVarianceRule
{
    public List<string> Check(RollingGarnishmentVariance item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling garnishment variance.</summary>
public sealed record RollingGarnishmentVarianceValidator(List<IRollingGarnishmentVarianceRule> Rules)
{
    public List<string> Validate(RollingGarnishmentVariance item)
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

    public void RequireValid(RollingGarnishmentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingGarnishmentVariance> items)
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

/// <summary>Functions of rolling garnishment variance rules.</summary>
public static class RollingGarnishmentVarianceRules
{
    public static RollingGarnishmentVarianceValidator DefaultRollingGarnishmentVarianceValidator()
    {
        return new RollingGarnishmentVarianceValidator(
            new List<IRollingGarnishmentVarianceRule>
            {
                new RollingGarnishmentVarianceRequiredCodeRule("rolling garnishment variance needs this field"),
                new RollingGarnishmentVarianceRequiredDescriptionRule("rolling garnishment variance needs an entry"),
                new RollingGarnishmentVarianceTitleLengthRule(1000),
                new RollingGarnishmentVarianceHoursRangeRule(9.3, 3533.6),
            });
    }
}
