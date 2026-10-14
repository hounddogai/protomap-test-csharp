namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>A rule a statement of work template must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkTemplateRule
{
    List<string> Check(StatementOfWorkTemplate item);
}

public sealed record StatementOfWorkTemplateActiveGuardRule(double Limit) : IStatementOfWorkTemplateRule
{
    public List<string> Check(StatementOfWorkTemplate item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTemplateCodeLengthRule(int MaxLength) : IStatementOfWorkTemplateRule
{
    public List<string> Check(StatementOfWorkTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTemplateDepartmentLengthRule(int MaxLength) : IStatementOfWorkTemplateRule
{
    public List<string> Check(StatementOfWorkTemplate item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTemplateCodePrefixRule(string Prefix) : IStatementOfWorkTemplateRule
{
    public List<string> Check(StatementOfWorkTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTemplateLabelLengthRule(int MaxLength) : IStatementOfWorkTemplateRule
{
    public List<string> Check(StatementOfWorkTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work template.</summary>
public sealed record StatementOfWorkTemplateValidator(List<IStatementOfWorkTemplateRule> Rules)
{
    public List<string> Validate(StatementOfWorkTemplate item)
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

    public void RequireValid(StatementOfWorkTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkTemplate> items)
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

/// <summary>Functions of statement of work template rules.</summary>
public static class StatementOfWorkTemplateRules
{
    public static StatementOfWorkTemplateValidator DefaultStatementOfWorkTemplateValidator()
    {
        return new StatementOfWorkTemplateValidator(
            new List<IStatementOfWorkTemplateRule>
            {
                new StatementOfWorkTemplateActiveGuardRule(3.3),
                new StatementOfWorkTemplateCodeLengthRule(16),
                new StatementOfWorkTemplateDepartmentLengthRule(1000),
                new StatementOfWorkTemplateCodePrefixRule("X"),
                new StatementOfWorkTemplateLabelLengthRule(255),
            });
    }
}
