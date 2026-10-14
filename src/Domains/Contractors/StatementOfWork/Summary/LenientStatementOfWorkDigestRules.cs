namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient statement of work digest must pass. Check returns its problems, or none.</summary>
public interface ILenientStatementOfWorkDigestRule
{
    List<string> Check(LenientStatementOfWorkDigest item);
}

public sealed record LenientStatementOfWorkDigestCodePrefixRule(string Prefix) : ILenientStatementOfWorkDigestRule
{
    public List<string> Check(LenientStatementOfWorkDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkDigestRatioRangeRule(double Min, double Max) : ILenientStatementOfWorkDigestRule
{
    public List<string> Check(LenientStatementOfWorkDigest item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientStatementOfWorkDigestAllowedTitleRule(List<string> Allowed) : ILenientStatementOfWorkDigestRule
{
    public List<string> Check(LenientStatementOfWorkDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkDigestTitleLengthRule(int MaxLength) : ILenientStatementOfWorkDigestRule
{
    public List<string> Check(LenientStatementOfWorkDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient statement of work digest.</summary>
public sealed record LenientStatementOfWorkDigestValidator(List<ILenientStatementOfWorkDigestRule> Rules)
{
    public List<string> Validate(LenientStatementOfWorkDigest item)
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

    public void RequireValid(LenientStatementOfWorkDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientStatementOfWorkDigest> items)
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

/// <summary>Functions of lenient statement of work digest rules.</summary>
public static class LenientStatementOfWorkDigestRules
{
    public static LenientStatementOfWorkDigestValidator DefaultLenientStatementOfWorkDigestValidator()
    {
        return new LenientStatementOfWorkDigestValidator(
            new List<ILenientStatementOfWorkDigestRule>
            {
                new LenientStatementOfWorkDigestCodePrefixRule("X"),
                new LenientStatementOfWorkDigestRatioRangeRule(0.0, 1961.1),
                new LenientStatementOfWorkDigestAllowedTitleRule(
                    new List<string> { "strict", "batched", "seasonal" }),
                new LenientStatementOfWorkDigestTitleLengthRule(32),
            });
    }
}
