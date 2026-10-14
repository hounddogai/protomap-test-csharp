namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled statement of work record must pass. Check returns its problems, or none.</summary>
public interface IScheduledStatementOfWorkRecordRule
{
    List<string> Check(ScheduledStatementOfWorkRecord item);
}

public sealed record ScheduledStatementOfWorkRecordRequiredSummaryRule(string Message) : IScheduledStatementOfWorkRecordRule
{
    public List<string> Check(ScheduledStatementOfWorkRecord item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkRecordVersionRangeRule(int Min, int Max) : IScheduledStatementOfWorkRecordRule
{
    public List<string> Check(ScheduledStatementOfWorkRecord item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkRecordRequiredNameRule(string Message) : IScheduledStatementOfWorkRecordRule
{
    public List<string> Check(ScheduledStatementOfWorkRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkRecordMinimumRangeRule(double Min, double Max) : IScheduledStatementOfWorkRecordRule
{
    public List<string> Check(ScheduledStatementOfWorkRecord item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkRecordAllowedBadgeRule(List<string> Allowed) : IScheduledStatementOfWorkRecordRule
{
    public List<string> Check(ScheduledStatementOfWorkRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled statement of work record.</summary>
public sealed record ScheduledStatementOfWorkRecordValidator(List<IScheduledStatementOfWorkRecordRule> Rules)
{
    public List<string> Validate(ScheduledStatementOfWorkRecord item)
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

    public void RequireValid(ScheduledStatementOfWorkRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledStatementOfWorkRecord> items)
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

/// <summary>Functions of scheduled statement of work record rules.</summary>
public static class ScheduledStatementOfWorkRecordRules
{
    public static ScheduledStatementOfWorkRecordValidator DefaultScheduledStatementOfWorkRecordValidator()
    {
        return new ScheduledStatementOfWorkRecordValidator(
            new List<IScheduledStatementOfWorkRecordRule>
            {
                new ScheduledStatementOfWorkRecordRequiredSummaryRule(
                    "scheduled statement of work record needs an entry"),
                new ScheduledStatementOfWorkRecordVersionRangeRule(80, 255),
                new ScheduledStatementOfWorkRecordRequiredNameRule(
                    "scheduled statement of work record needs an entry"),
                new ScheduledStatementOfWorkRecordMinimumRangeRule(8.1, 823.3),
                new ScheduledStatementOfWorkRecordAllowedBadgeRule(
                    new List<string> { "fallback", "adaptive", "regional" }),
            });
    }
}
