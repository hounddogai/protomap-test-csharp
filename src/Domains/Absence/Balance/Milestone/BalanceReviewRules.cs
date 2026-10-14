namespace Acme.Workforce.Absence.Balance.Milestone;

/// <summary>A rule a balance review must pass. Check returns its problems, or none.</summary>
public interface IBalanceReviewRule
{
    List<string> Check(BalanceReview item);
}

public sealed record BalanceReviewRequiredSalaryBandRule(string Message) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceReviewCodePrefixRule(string Prefix) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceReviewRequiredLabelRule(string Message) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceReviewLabelLengthRule(int MaxLength) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceReviewSalaryBandLengthRule(int MaxLength) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceReviewMarginRangeRule(double Min, double Max) : IBalanceReviewRule
{
    public List<string> Check(BalanceReview item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a balance review.</summary>
public sealed record BalanceReviewValidator(List<IBalanceReviewRule> Rules)
{
    public List<string> Validate(BalanceReview item)
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

    public void RequireValid(BalanceReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceReview> items)
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

/// <summary>Functions of balance review rules.</summary>
public static class BalanceReviewRules
{
    public static BalanceReviewValidator DefaultBalanceReviewValidator()
    {
        return new BalanceReviewValidator(
            new List<IBalanceReviewRule>
            {
                new BalanceReviewRequiredSalaryBandRule("balance review needs a value"),
                new BalanceReviewCodePrefixRule("REF"),
                new BalanceReviewRequiredLabelRule("balance review needs this field"),
                new BalanceReviewLabelLengthRule(64),
                new BalanceReviewSalaryBandLengthRule(64),
                new BalanceReviewMarginRangeRule(8.8, 4813.1),
            });
    }
}
