namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default statement of work variance must pass. Check returns its problems, or none.</summary>
public interface IDefaultStatementOfWorkVarianceRule
{
    List<string> Check(DefaultStatementOfWorkVariance item);
}

public sealed record DefaultStatementOfWorkVarianceNameLengthRule(int MaxLength) : IDefaultStatementOfWorkVarianceRule
{
    public List<string> Check(DefaultStatementOfWorkVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkVarianceAllowedNameRule(List<string> Allowed) : IDefaultStatementOfWorkVarianceRule
{
    public List<string> Check(DefaultStatementOfWorkVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkVarianceCodeLengthRule(int MaxLength) : IDefaultStatementOfWorkVarianceRule
{
    public List<string> Check(DefaultStatementOfWorkVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkVarianceCodePrefixRule(string Prefix) : IDefaultStatementOfWorkVarianceRule
{
    public List<string> Check(DefaultStatementOfWorkVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkVarianceRequiredJobCodeRule(string Message) : IDefaultStatementOfWorkVarianceRule
{
    public List<string> Check(DefaultStatementOfWorkVariance item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default statement of work variance.</summary>
public sealed record DefaultStatementOfWorkVarianceValidator(List<IDefaultStatementOfWorkVarianceRule> Rules)
{
    public List<string> Validate(DefaultStatementOfWorkVariance item)
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

    public void RequireValid(DefaultStatementOfWorkVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultStatementOfWorkVariance> items)
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

/// <summary>Functions of default statement of work variance rules.</summary>
public static class DefaultStatementOfWorkVarianceRules
{
    public static DefaultStatementOfWorkVarianceValidator DefaultDefaultStatementOfWorkVarianceValidator()
    {
        return new DefaultStatementOfWorkVarianceValidator(
            new List<IDefaultStatementOfWorkVarianceRule>
            {
                new DefaultStatementOfWorkVarianceNameLengthRule(32),
                new DefaultStatementOfWorkVarianceAllowedNameRule(new List<string> { "adaptive" }),
                new DefaultStatementOfWorkVarianceCodeLengthRule(3600),
                new DefaultStatementOfWorkVarianceCodePrefixRule("ACM"),
                new DefaultStatementOfWorkVarianceRequiredJobCodeRule(
                    "default statement of work variance needs this field"),
            });
    }
}
