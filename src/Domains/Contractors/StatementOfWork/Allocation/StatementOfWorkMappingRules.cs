namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work mapping must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkMappingRule
{
    List<string> Check(StatementOfWorkMapping item);
}

public sealed record StatementOfWorkMappingLocationLengthRule(int MaxLength) : IStatementOfWorkMappingRule
{
    public List<string> Check(StatementOfWorkMapping item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkMappingCodePrefixRule(string Prefix) : IStatementOfWorkMappingRule
{
    public List<string> Check(StatementOfWorkMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkMappingAllowedCodeRule(List<string> Allowed) : IStatementOfWorkMappingRule
{
    public List<string> Check(StatementOfWorkMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkMappingRequiredCodeRule(string Message) : IStatementOfWorkMappingRule
{
    public List<string> Check(StatementOfWorkMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkMappingRequiredNameRule(string Message) : IStatementOfWorkMappingRule
{
    public List<string> Check(StatementOfWorkMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work mapping.</summary>
public sealed record StatementOfWorkMappingValidator(List<IStatementOfWorkMappingRule> Rules)
{
    public List<string> Validate(StatementOfWorkMapping item)
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

    public void RequireValid(StatementOfWorkMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkMapping> items)
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

/// <summary>Functions of statement of work mapping rules.</summary>
public static class StatementOfWorkMappingRules
{
    public static StatementOfWorkMappingValidator DefaultStatementOfWorkMappingValidator()
    {
        return new StatementOfWorkMappingValidator(
            new List<IStatementOfWorkMappingRule>
            {
                new StatementOfWorkMappingLocationLengthRule(1000),
                new StatementOfWorkMappingCodePrefixRule("X"),
                new StatementOfWorkMappingAllowedCodeRule(new List<string> { "primary" }),
                new StatementOfWorkMappingRequiredCodeRule("statement of work mapping needs an entry"),
                new StatementOfWorkMappingRequiredNameRule("statement of work mapping needs an entry"),
            });
    }
}
