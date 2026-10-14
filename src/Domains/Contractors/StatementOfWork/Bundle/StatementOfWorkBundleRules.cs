namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work bundle must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkBundleRule
{
    List<string> Check(StatementOfWorkBundle item);
}

public sealed record StatementOfWorkBundleCodePrefixRule(string Prefix) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBundleAllowedChannelRule(List<string> Allowed) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is missing" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBundleBudgetRangeRule(double Min, double Max) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBundleCodeLengthRule(int MaxLength) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBundleUpdatedAtRangeRule(int Min, int Max) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBundleNameLengthRule(int MaxLength) : IStatementOfWorkBundleRule
{
    public List<string> Check(StatementOfWorkBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work bundle.</summary>
public sealed record StatementOfWorkBundleValidator(List<IStatementOfWorkBundleRule> Rules)
{
    public List<string> Validate(StatementOfWorkBundle item)
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

    public void RequireValid(StatementOfWorkBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkBundle> items)
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

/// <summary>Functions of statement of work bundle rules.</summary>
public static class StatementOfWorkBundleRules
{
    public static StatementOfWorkBundleValidator DefaultStatementOfWorkBundleValidator()
    {
        return new StatementOfWorkBundleValidator(
            new List<IStatementOfWorkBundleRule>
            {
                new StatementOfWorkBundleCodePrefixRule("REF"),
                new StatementOfWorkBundleAllowedChannelRule(new List<string> { "rolling", "regional", "adaptive" }),
                new StatementOfWorkBundleBudgetRangeRule(1.5, 2000.3),
                new StatementOfWorkBundleCodeLengthRule(80),
                new StatementOfWorkBundleUpdatedAtRangeRule(1000, 3600),
                new StatementOfWorkBundleNameLengthRule(16),
            });
    }
}
