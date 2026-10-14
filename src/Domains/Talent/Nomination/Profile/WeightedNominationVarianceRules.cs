namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted nomination variance must pass. Check returns its problems, or none.</summary>
public interface IWeightedNominationVarianceRule
{
    List<string> Check(WeightedNominationVariance item);
}

public sealed record WeightedNominationVarianceAmountRangeRule(double Min, double Max) : IWeightedNominationVarianceRule
{
    public List<string> Check(WeightedNominationVariance item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedNominationVarianceCodePrefixRule(string Prefix) : IWeightedNominationVarianceRule
{
    public List<string> Check(WeightedNominationVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedNominationVarianceCodeLengthRule(int MaxLength) : IWeightedNominationVarianceRule
{
    public List<string> Check(WeightedNominationVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedNominationVarianceAllowedTitleRule(List<string> Allowed) : IWeightedNominationVarianceRule
{
    public List<string> Check(WeightedNominationVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted nomination variance.</summary>
public sealed record WeightedNominationVarianceValidator(List<IWeightedNominationVarianceRule> Rules)
{
    public List<string> Validate(WeightedNominationVariance item)
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

    public void RequireValid(WeightedNominationVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedNominationVariance> items)
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

/// <summary>Functions of weighted nomination variance rules.</summary>
public static class WeightedNominationVarianceRules
{
    public static WeightedNominationVarianceValidator DefaultWeightedNominationVarianceValidator()
    {
        return new WeightedNominationVarianceValidator(
            new List<IWeightedNominationVarianceRule>
            {
                new WeightedNominationVarianceAmountRangeRule(8.9, 2531.2),
                new WeightedNominationVarianceCodePrefixRule("OPS"),
                new WeightedNominationVarianceCodeLengthRule(120),
                new WeightedNominationVarianceAllowedTitleRule(
                    new List<string> { "seasonal", "weighted", "seasonal" }),
            });
    }
}
