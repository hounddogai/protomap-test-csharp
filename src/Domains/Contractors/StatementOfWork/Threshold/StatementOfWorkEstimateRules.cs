namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>A rule a statement of work estimate must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkEstimateRule
{
    List<string> Check(StatementOfWorkEstimate item);
}

public sealed record StatementOfWorkEstimateRequiredLabelRule(string Message) : IStatementOfWorkEstimateRule
{
    public List<string> Check(StatementOfWorkEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkEstimateRequiredDescriptionRule(string Message) : IStatementOfWorkEstimateRule
{
    public List<string> Check(StatementOfWorkEstimate item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkEstimateRequiredCodeRule(string Message) : IStatementOfWorkEstimateRule
{
    public List<string> Check(StatementOfWorkEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkEstimateChecksumLengthRule(int MaxLength) : IStatementOfWorkEstimateRule
{
    public List<string> Check(StatementOfWorkEstimate item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkEstimateCodePrefixRule(string Prefix) : IStatementOfWorkEstimateRule
{
    public List<string> Check(StatementOfWorkEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work estimate.</summary>
public sealed record StatementOfWorkEstimateValidator(List<IStatementOfWorkEstimateRule> Rules)
{
    public List<string> Validate(StatementOfWorkEstimate item)
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

    public void RequireValid(StatementOfWorkEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkEstimate> items)
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

/// <summary>Functions of statement of work estimate rules.</summary>
public static class StatementOfWorkEstimateRules
{
    public static StatementOfWorkEstimateValidator DefaultStatementOfWorkEstimateValidator()
    {
        return new StatementOfWorkEstimateValidator(
            new List<IStatementOfWorkEstimateRule>
            {
                new StatementOfWorkEstimateRequiredLabelRule("statement of work estimate needs this field"),
                new StatementOfWorkEstimateRequiredDescriptionRule("statement of work estimate needs this field"),
                new StatementOfWorkEstimateRequiredCodeRule("statement of work estimate needs this field"),
                new StatementOfWorkEstimateChecksumLengthRule(80),
                new StatementOfWorkEstimateCodePrefixRule("REF"),
            });
    }
}
