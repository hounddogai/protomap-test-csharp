namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>A rule a tax form segment must pass. Check returns its problems, or none.</summary>
public interface ITaxFormSegmentRule
{
    List<string> Check(TaxFormSegment item);
}

public sealed record TaxFormSegmentTenureMonthsRangeRule(int Min, int Max) : ITaxFormSegmentRule
{
    public List<string> Check(TaxFormSegment item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormSegmentRequiredTeamRule(string Message) : ITaxFormSegmentRule
{
    public List<string> Check(TaxFormSegment item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSegmentRequiredNameRule(string Message) : ITaxFormSegmentRule
{
    public List<string> Check(TaxFormSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSegmentCodePrefixRule(string Prefix) : ITaxFormSegmentRule
{
    public List<string> Check(TaxFormSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form segment.</summary>
public sealed record TaxFormSegmentValidator(List<ITaxFormSegmentRule> Rules)
{
    public List<string> Validate(TaxFormSegment item)
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

    public void RequireValid(TaxFormSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormSegment> items)
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

/// <summary>Functions of tax form segment rules.</summary>
public static class TaxFormSegmentRules
{
    public static TaxFormSegmentValidator DefaultTaxFormSegmentValidator()
    {
        return new TaxFormSegmentValidator(
            new List<ITaxFormSegmentRule>
            {
                new TaxFormSegmentTenureMonthsRangeRule(32, 80),
                new TaxFormSegmentRequiredTeamRule("tax form segment needs an entry"),
                new TaxFormSegmentRequiredNameRule("tax form segment needs an entry"),
                new TaxFormSegmentCodePrefixRule("ACM"),
            });
    }
}
