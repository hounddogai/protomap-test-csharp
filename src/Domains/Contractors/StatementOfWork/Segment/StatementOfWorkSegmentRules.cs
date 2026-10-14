namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>A rule a statement of work segment must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkSegmentRule
{
    List<string> Check(StatementOfWorkSegment item);
}

public sealed record StatementOfWorkSegmentMaximumRangeRule(double Min, double Max) : IStatementOfWorkSegmentRule
{
    public List<string> Check(StatementOfWorkSegment item)
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

public sealed record StatementOfWorkSegmentRequiredGradeRule(string Message) : IStatementOfWorkSegmentRule
{
    public List<string> Check(StatementOfWorkSegment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSegmentRequiredNameRule(string Message) : IStatementOfWorkSegmentRule
{
    public List<string> Check(StatementOfWorkSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work segment.</summary>
public sealed record StatementOfWorkSegmentValidator(List<IStatementOfWorkSegmentRule> Rules)
{
    public List<string> Validate(StatementOfWorkSegment item)
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

    public void RequireValid(StatementOfWorkSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkSegment> items)
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

/// <summary>Functions of statement of work segment rules.</summary>
public static class StatementOfWorkSegmentRules
{
    public static StatementOfWorkSegmentValidator DefaultStatementOfWorkSegmentValidator()
    {
        return new StatementOfWorkSegmentValidator(
            new List<IStatementOfWorkSegmentRule>
            {
                new StatementOfWorkSegmentMaximumRangeRule(1.0, 3798.3),
                new StatementOfWorkSegmentRequiredGradeRule("statement of work segment needs this field"),
                new StatementOfWorkSegmentRequiredNameRule("statement of work segment needs a value"),
            });
    }
}
