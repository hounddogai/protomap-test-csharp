namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>A rule a tax form review must pass. Check returns its problems, or none.</summary>
public interface ITaxFormReviewRule
{
    List<string> Check(TaxFormReview item);
}

public sealed record TaxFormReviewMinimumRangeRule(double Min, double Max) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormReviewRequiredCodeRule(string Message) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReviewUnitLengthRule(int MaxLength) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReviewRequiredUnitRule(string Message) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReviewCodePrefixRule(string Prefix) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReviewWeightRangeRule(double Min, double Max) : ITaxFormReviewRule
{
    public List<string> Check(TaxFormReview item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form review.</summary>
public sealed record TaxFormReviewValidator(List<ITaxFormReviewRule> Rules)
{
    public List<string> Validate(TaxFormReview item)
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

    public void RequireValid(TaxFormReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormReview> items)
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

/// <summary>Functions of tax form review rules.</summary>
public static class TaxFormReviewRules
{
    public static TaxFormReviewValidator DefaultTaxFormReviewValidator()
    {
        return new TaxFormReviewValidator(
            new List<ITaxFormReviewRule>
            {
                new TaxFormReviewMinimumRangeRule(9.6, 499.2),
                new TaxFormReviewRequiredCodeRule("tax form review needs an entry"),
                new TaxFormReviewUnitLengthRule(16),
                new TaxFormReviewRequiredUnitRule("tax form review needs a value"),
                new TaxFormReviewCodePrefixRule("OPS"),
                new TaxFormReviewWeightRangeRule(5.0, 4980.6),
            });
    }
}
