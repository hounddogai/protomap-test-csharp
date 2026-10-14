namespace Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>A rule a statement of work group must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkGroupRule
{
    List<string> Check(StatementOfWorkGroup item);
}

public sealed record StatementOfWorkGroupCodeLengthRule(int MaxLength) : IStatementOfWorkGroupRule
{
    public List<string> Check(StatementOfWorkGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkGroupNameLengthRule(int MaxLength) : IStatementOfWorkGroupRule
{
    public List<string> Check(StatementOfWorkGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkGroupLabelLengthRule(int MaxLength) : IStatementOfWorkGroupRule
{
    public List<string> Check(StatementOfWorkGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkGroupRequiredNameRule(string Message) : IStatementOfWorkGroupRule
{
    public List<string> Check(StatementOfWorkGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkGroupCodePrefixRule(string Prefix) : IStatementOfWorkGroupRule
{
    public List<string> Check(StatementOfWorkGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work group.</summary>
public sealed record StatementOfWorkGroupValidator(List<IStatementOfWorkGroupRule> Rules)
{
    public List<string> Validate(StatementOfWorkGroup item)
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

    public void RequireValid(StatementOfWorkGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkGroup> items)
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

/// <summary>Functions of statement of work group rules.</summary>
public static class StatementOfWorkGroupRules
{
    public static StatementOfWorkGroupValidator DefaultStatementOfWorkGroupValidator()
    {
        return new StatementOfWorkGroupValidator(
            new List<IStatementOfWorkGroupRule>
            {
                new StatementOfWorkGroupCodeLengthRule(32),
                new StatementOfWorkGroupNameLengthRule(3600),
                new StatementOfWorkGroupLabelLengthRule(1000),
                new StatementOfWorkGroupRequiredNameRule("statement of work group needs a value"),
                new StatementOfWorkGroupCodePrefixRule("OPS"),
            });
    }
}
