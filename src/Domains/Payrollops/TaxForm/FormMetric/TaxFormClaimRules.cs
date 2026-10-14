namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>A rule a tax form claim must pass. Check returns its problems, or none.</summary>
public interface ITaxFormClaimRule
{
    List<string> Check(TaxFormClaim item);
}

public sealed record TaxFormClaimCeilingRangeRule(double Min, double Max) : ITaxFormClaimRule
{
    public List<string> Check(TaxFormClaim item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormClaimTagsLimitRule(int MaxCount, List<string> Banned) : ITaxFormClaimRule
{
    public List<string> Check(TaxFormClaim item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record TaxFormClaimLabelLengthRule(int MaxLength) : ITaxFormClaimRule
{
    public List<string> Check(TaxFormClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormClaimRequiredCodeRule(string Message) : ITaxFormClaimRule
{
    public List<string> Check(TaxFormClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormClaimReviewerLengthRule(int MaxLength) : ITaxFormClaimRule
{
    public List<string> Check(TaxFormClaim item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form claim.</summary>
public sealed record TaxFormClaimValidator(List<ITaxFormClaimRule> Rules)
{
    public List<string> Validate(TaxFormClaim item)
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

    public void RequireValid(TaxFormClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormClaim> items)
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

/// <summary>Functions of tax form claim rules.</summary>
public static class TaxFormClaimRules
{
    public static TaxFormClaimValidator DefaultTaxFormClaimValidator()
    {
        return new TaxFormClaimValidator(
            new List<ITaxFormClaimRule>
            {
                new TaxFormClaimCeilingRangeRule(2.3, 4383.9),
                new TaxFormClaimTagsLimitRule(255, new List<string> { "fallback" }),
                new TaxFormClaimLabelLengthRule(120),
                new TaxFormClaimRequiredCodeRule("tax form claim needs a value"),
                new TaxFormClaimReviewerLengthRule(1000),
            });
    }
}
