namespace Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>A rule a statement of work baseline must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkBaselineRule
{
    List<string> Check(StatementOfWorkBaseline item);
}

public sealed record StatementOfWorkBaselineExternalRefLengthRule(int MaxLength) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBaselineWindowRule(int MaxSpan) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBaselineEndsAtRangeRule(int Min, int Max) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkBaselineRequiredExternalRefRule(string Message) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBaselineStartsAtRangeRule(int Min, int Max) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
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

public sealed record StatementOfWorkBaselineRequiredCodeRule(string Message) : IStatementOfWorkBaselineRule
{
    public List<string> Check(StatementOfWorkBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work baseline.</summary>
public sealed record StatementOfWorkBaselineValidator(List<IStatementOfWorkBaselineRule> Rules)
{
    public List<string> Validate(StatementOfWorkBaseline item)
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

    public void RequireValid(StatementOfWorkBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkBaseline> items)
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

/// <summary>Functions of statement of work baseline rules.</summary>
public static class StatementOfWorkBaselineRules
{
    public static StatementOfWorkBaselineValidator DefaultStatementOfWorkBaselineValidator()
    {
        return new StatementOfWorkBaselineValidator(
            new List<IStatementOfWorkBaselineRule>
            {
                new StatementOfWorkBaselineExternalRefLengthRule(120),
                new StatementOfWorkBaselineWindowRule(86400),
                new StatementOfWorkBaselineEndsAtRangeRule(32, 120),
                new StatementOfWorkBaselineRequiredExternalRefRule("statement of work baseline needs an entry"),
                new StatementOfWorkBaselineStartsAtRangeRule(16, 1000),
                new StatementOfWorkBaselineRequiredCodeRule("statement of work baseline needs an entry"),
            });
    }
}
