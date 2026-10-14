namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance variance must pass. Check returns its problems, or none.</summary>
public interface IBalanceVarianceRule
{
    List<string> Check(BalanceVariance item);
}

public sealed record BalanceVarianceAllowedSummaryRule(List<string> Allowed) : IBalanceVarianceRule
{
    public List<string> Check(BalanceVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BalanceVarianceSummaryLengthRule(int MaxLength) : IBalanceVarianceRule
{
    public List<string> Check(BalanceVariance item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceVarianceMaximumRangeRule(double Min, double Max) : IBalanceVarianceRule
{
    public List<string> Check(BalanceVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceVarianceRequiredCodeRule(string Message) : IBalanceVarianceRule
{
    public List<string> Check(BalanceVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceVarianceNameLengthRule(int MaxLength) : IBalanceVarianceRule
{
    public List<string> Check(BalanceVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance variance.</summary>
public sealed record BalanceVarianceValidator(List<IBalanceVarianceRule> Rules)
{
    public List<string> Validate(BalanceVariance item)
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

    public void RequireValid(BalanceVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceVariance> items)
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

/// <summary>Functions of balance variance rules.</summary>
public static class BalanceVarianceRules
{
    public static BalanceVarianceValidator DefaultBalanceVarianceValidator()
    {
        return new BalanceVarianceValidator(
            new List<IBalanceVarianceRule>
            {
                new BalanceVarianceAllowedSummaryRule(new List<string> { "pending" }),
                new BalanceVarianceSummaryLengthRule(86400),
                new BalanceVarianceMaximumRangeRule(3.0, 3562.1),
                new BalanceVarianceRequiredCodeRule("balance variance needs a value"),
                new BalanceVarianceNameLengthRule(32),
            });
    }
}
