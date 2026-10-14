namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>A rule a default statement of work mapping must pass. Check returns its problems, or none.</summary>
public interface IDefaultStatementOfWorkMappingRule
{
    List<string> Check(DefaultStatementOfWorkMapping item);
}

public sealed record DefaultStatementOfWorkMappingRequiredLabelRule(string Message) : IDefaultStatementOfWorkMappingRule
{
    public List<string> Check(DefaultStatementOfWorkMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMappingRequiredSourceRule(string Message) : IDefaultStatementOfWorkMappingRule
{
    public List<string> Check(DefaultStatementOfWorkMapping item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMappingLabelLengthRule(int MaxLength) : IDefaultStatementOfWorkMappingRule
{
    public List<string> Check(DefaultStatementOfWorkMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default statement of work mapping.</summary>
public sealed record DefaultStatementOfWorkMappingValidator(List<IDefaultStatementOfWorkMappingRule> Rules)
{
    public List<string> Validate(DefaultStatementOfWorkMapping item)
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

    public void RequireValid(DefaultStatementOfWorkMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultStatementOfWorkMapping> items)
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

/// <summary>Functions of default statement of work mapping rules.</summary>
public static class DefaultStatementOfWorkMappingRules
{
    public static DefaultStatementOfWorkMappingValidator DefaultDefaultStatementOfWorkMappingValidator()
    {
        return new DefaultStatementOfWorkMappingValidator(
            new List<IDefaultStatementOfWorkMappingRule>
            {
                new DefaultStatementOfWorkMappingRequiredLabelRule(
                    "default statement of work mapping needs an entry"),
                new DefaultStatementOfWorkMappingRequiredSourceRule(
                    "default statement of work mapping needs a value"),
                new DefaultStatementOfWorkMappingLabelLengthRule(3600),
            });
    }
}
