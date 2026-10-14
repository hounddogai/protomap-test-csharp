namespace Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>A rule a scheduled exception ledger must pass. Check returns its problems, or none.</summary>
public interface IScheduledExceptionLedgerRule
{
    List<string> Check(ScheduledExceptionLedger item);
}

public sealed record ScheduledExceptionLedgerRequiredCodeRule(string Message) : IScheduledExceptionLedgerRule
{
    public List<string> Check(ScheduledExceptionLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionLedgerHoursRangeRule(double Min, double Max) : IScheduledExceptionLedgerRule
{
    public List<string> Check(ScheduledExceptionLedger item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionLedgerMaximumRangeRule(double Min, double Max) : IScheduledExceptionLedgerRule
{
    public List<string> Check(ScheduledExceptionLedger item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled exception ledger.</summary>
public sealed record ScheduledExceptionLedgerValidator(List<IScheduledExceptionLedgerRule> Rules)
{
    public List<string> Validate(ScheduledExceptionLedger item)
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

    public void RequireValid(ScheduledExceptionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledExceptionLedger> items)
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

/// <summary>Functions of scheduled exception ledger rules.</summary>
public static class ScheduledExceptionLedgerRules
{
    public static ScheduledExceptionLedgerValidator DefaultScheduledExceptionLedgerValidator()
    {
        return new ScheduledExceptionLedgerValidator(
            new List<IScheduledExceptionLedgerRule>
            {
                new ScheduledExceptionLedgerRequiredCodeRule("scheduled exception ledger needs this field"),
                new ScheduledExceptionLedgerHoursRangeRule(9.4, 4919.2),
                new ScheduledExceptionLedgerMaximumRangeRule(7.7, 3013.4),
            });
    }
}
