namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>A rule a weighted visitor baseline must pass. Check returns its problems, or none.</summary>
public interface IWeightedVisitorBaselineRule
{
    List<string> Check(WeightedVisitorBaseline item);
}

public sealed record WeightedVisitorBaselineDiscountRangeRule(double Min, double Max) : IWeightedVisitorBaselineRule
{
    public List<string> Check(WeightedVisitorBaseline item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedVisitorBaselineLabelLengthRule(int MaxLength) : IWeightedVisitorBaselineRule
{
    public List<string> Check(WeightedVisitorBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedVisitorBaselineCodePrefixRule(string Prefix) : IWeightedVisitorBaselineRule
{
    public List<string> Check(WeightedVisitorBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted visitor baseline.</summary>
public sealed record WeightedVisitorBaselineValidator(List<IWeightedVisitorBaselineRule> Rules)
{
    public List<string> Validate(WeightedVisitorBaseline item)
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

    public void RequireValid(WeightedVisitorBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedVisitorBaseline> items)
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

/// <summary>Functions of weighted visitor baseline rules.</summary>
public static class WeightedVisitorBaselineRules
{
    public static WeightedVisitorBaselineValidator DefaultWeightedVisitorBaselineValidator()
    {
        return new WeightedVisitorBaselineValidator(
            new List<IWeightedVisitorBaselineRule>
            {
                new WeightedVisitorBaselineDiscountRangeRule(7.0, 790.5),
                new WeightedVisitorBaselineLabelLengthRule(255),
                new WeightedVisitorBaselineCodePrefixRule("REF"),
            });
    }
}
