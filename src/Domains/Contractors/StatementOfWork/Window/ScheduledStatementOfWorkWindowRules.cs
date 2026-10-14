namespace Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>A rule a scheduled statement of work window must pass. Check returns its problems, or none.</summary>
public interface IScheduledStatementOfWorkWindowRule
{
    List<string> Check(ScheduledStatementOfWorkWindow item);
}

public sealed record ScheduledStatementOfWorkWindowCountryLengthRule(int MaxLength) : IScheduledStatementOfWorkWindowRule
{
    public List<string> Check(ScheduledStatementOfWorkWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkWindowLabelLengthRule(int MaxLength) : IScheduledStatementOfWorkWindowRule
{
    public List<string> Check(ScheduledStatementOfWorkWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkWindowCodePrefixRule(string Prefix) : IScheduledStatementOfWorkWindowRule
{
    public List<string> Check(ScheduledStatementOfWorkWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled statement of work window.</summary>
public sealed record ScheduledStatementOfWorkWindowValidator(List<IScheduledStatementOfWorkWindowRule> Rules)
{
    public List<string> Validate(ScheduledStatementOfWorkWindow item)
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

    public void RequireValid(ScheduledStatementOfWorkWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledStatementOfWorkWindow> items)
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

/// <summary>Functions of scheduled statement of work window rules.</summary>
public static class ScheduledStatementOfWorkWindowRules
{
    public static ScheduledStatementOfWorkWindowValidator DefaultScheduledStatementOfWorkWindowValidator()
    {
        return new ScheduledStatementOfWorkWindowValidator(
            new List<IScheduledStatementOfWorkWindowRule>
            {
                new ScheduledStatementOfWorkWindowCountryLengthRule(3600),
                new ScheduledStatementOfWorkWindowLabelLengthRule(16),
                new ScheduledStatementOfWorkWindowCodePrefixRule("OPS"),
            });
    }
}
