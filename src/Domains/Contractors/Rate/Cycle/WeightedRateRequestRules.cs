namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted rate request must pass. Check returns its problems, or none.</summary>
public interface IWeightedRateRequestRule
{
    List<string> Check(WeightedRateRequest item);
}

public sealed record WeightedRateRequestAttemptsRangeRule(int Min, int Max) : IWeightedRateRequestRule
{
    public List<string> Check(WeightedRateRequest item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateRequestLocationLengthRule(int MaxLength) : IWeightedRateRequestRule
{
    public List<string> Check(WeightedRateRequest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateRequestAllowedLocationRule(List<string> Allowed) : IWeightedRateRequestRule
{
    public List<string> Check(WeightedRateRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateRequestRequiredReviewerRule(string Message) : IWeightedRateRequestRule
{
    public List<string> Check(WeightedRateRequest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate request.</summary>
public sealed record WeightedRateRequestValidator(List<IWeightedRateRequestRule> Rules)
{
    public List<string> Validate(WeightedRateRequest item)
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

    public void RequireValid(WeightedRateRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRateRequest> items)
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

/// <summary>Functions of weighted rate request rules.</summary>
public static class WeightedRateRequestRules
{
    public static WeightedRateRequestValidator DefaultWeightedRateRequestValidator()
    {
        return new WeightedRateRequestValidator(
            new List<IWeightedRateRequestRule>
            {
                new WeightedRateRequestAttemptsRangeRule(16, 80),
                new WeightedRateRequestLocationLengthRule(120),
                new WeightedRateRequestAllowedLocationRule(new List<string>()),
                new WeightedRateRequestRequiredReviewerRule("weighted rate request needs a value"),
            });
    }
}
