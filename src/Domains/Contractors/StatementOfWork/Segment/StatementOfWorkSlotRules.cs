namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>A rule a statement of work slot must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkSlotRule
{
    List<string> Check(StatementOfWorkSlot item);
}

public sealed record StatementOfWorkSlotRequiredGradeRule(string Message) : IStatementOfWorkSlotRule
{
    public List<string> Check(StatementOfWorkSlot item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSlotRegionLengthRule(int MaxLength) : IStatementOfWorkSlotRule
{
    public List<string> Check(StatementOfWorkSlot item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSlotGradeLengthRule(int MaxLength) : IStatementOfWorkSlotRule
{
    public List<string> Check(StatementOfWorkSlot item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work slot.</summary>
public sealed record StatementOfWorkSlotValidator(List<IStatementOfWorkSlotRule> Rules)
{
    public List<string> Validate(StatementOfWorkSlot item)
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

    public void RequireValid(StatementOfWorkSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkSlot> items)
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

/// <summary>Functions of statement of work slot rules.</summary>
public static class StatementOfWorkSlotRules
{
    public static StatementOfWorkSlotValidator DefaultStatementOfWorkSlotValidator()
    {
        return new StatementOfWorkSlotValidator(
            new List<IStatementOfWorkSlotRule>
            {
                new StatementOfWorkSlotRequiredGradeRule("statement of work slot needs an entry"),
                new StatementOfWorkSlotRegionLengthRule(255),
                new StatementOfWorkSlotGradeLengthRule(86400),
            });
    }
}
