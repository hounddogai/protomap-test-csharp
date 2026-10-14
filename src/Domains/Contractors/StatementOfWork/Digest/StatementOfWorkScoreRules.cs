namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work score must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkScoreRule
{
    List<string> Check(StatementOfWorkScore item);
}

public sealed record StatementOfWorkScoreNameLengthRule(int MaxLength) : IStatementOfWorkScoreRule
{
    public List<string> Check(StatementOfWorkScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkScoreUnitLengthRule(int MaxLength) : IStatementOfWorkScoreRule
{
    public List<string> Check(StatementOfWorkScore item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkScoreAllowedLocationRule(List<string> Allowed) : IStatementOfWorkScoreRule
{
    public List<string> Check(StatementOfWorkScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkScoreLimitRangeRule(int Min, int Max) : IStatementOfWorkScoreRule
{
    public List<string> Check(StatementOfWorkScore item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a statement of work score.</summary>
public sealed record StatementOfWorkScoreValidator(List<IStatementOfWorkScoreRule> Rules)
{
    public List<string> Validate(StatementOfWorkScore item)
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

    public void RequireValid(StatementOfWorkScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkScore> items)
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

/// <summary>Functions of statement of work score rules.</summary>
public static class StatementOfWorkScoreRules
{
    public static StatementOfWorkScoreValidator DefaultStatementOfWorkScoreValidator()
    {
        return new StatementOfWorkScoreValidator(
            new List<IStatementOfWorkScoreRule>
            {
                new StatementOfWorkScoreNameLengthRule(80),
                new StatementOfWorkScoreUnitLengthRule(80),
                new StatementOfWorkScoreAllowedLocationRule(new List<string>()),
                new StatementOfWorkScoreLimitRangeRule(86400, 86400),
            });
    }
}
