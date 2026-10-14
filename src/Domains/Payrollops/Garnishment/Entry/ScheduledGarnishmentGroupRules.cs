namespace Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>A rule a scheduled garnishment group must pass. Check returns its problems, or none.</summary>
public interface IScheduledGarnishmentGroupRule
{
    List<string> Check(ScheduledGarnishmentGroup item);
}

public sealed record ScheduledGarnishmentGroupRatingRangeRule(double Min, double Max) : IScheduledGarnishmentGroupRule
{
    public List<string> Check(ScheduledGarnishmentGroup item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledGarnishmentGroupMaximumRangeRule(double Min, double Max) : IScheduledGarnishmentGroupRule
{
    public List<string> Check(ScheduledGarnishmentGroup item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentGroupDescriptionLengthRule(int MaxLength) : IScheduledGarnishmentGroupRule
{
    public List<string> Check(ScheduledGarnishmentGroup item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentGroupRequiredNameRule(string Message) : IScheduledGarnishmentGroupRule
{
    public List<string> Check(ScheduledGarnishmentGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled garnishment group.</summary>
public sealed record ScheduledGarnishmentGroupValidator(List<IScheduledGarnishmentGroupRule> Rules)
{
    public List<string> Validate(ScheduledGarnishmentGroup item)
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

    public void RequireValid(ScheduledGarnishmentGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledGarnishmentGroup> items)
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

/// <summary>Functions of scheduled garnishment group rules.</summary>
public static class ScheduledGarnishmentGroupRules
{
    public static ScheduledGarnishmentGroupValidator DefaultScheduledGarnishmentGroupValidator()
    {
        return new ScheduledGarnishmentGroupValidator(
            new List<IScheduledGarnishmentGroupRule>
            {
                new ScheduledGarnishmentGroupRatingRangeRule(6.4, 4677.3),
                new ScheduledGarnishmentGroupMaximumRangeRule(7.1, 1358.7),
                new ScheduledGarnishmentGroupDescriptionLengthRule(120),
                new ScheduledGarnishmentGroupRequiredNameRule("scheduled garnishment group needs an entry"),
            });
    }
}
