namespace Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>A rule a pay band estimate must pass. Check returns its problems, or none.</summary>
public interface IPayBandEstimateRule
{
    List<string> Check(PayBandEstimate item);
}

public sealed record PayBandEstimateLabelLengthRule(int MaxLength) : IPayBandEstimateRule
{
    public List<string> Check(PayBandEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandEstimateSequenceRangeRule(int Min, int Max) : IPayBandEstimateRule
{
    public List<string> Check(PayBandEstimate item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandEstimateCodePrefixRule(string Prefix) : IPayBandEstimateRule
{
    public List<string> Check(PayBandEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandEstimateRequiredOwnerIdRule(string Message) : IPayBandEstimateRule
{
    public List<string> Check(PayBandEstimate item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandEstimateDurationMinutesRangeRule(int Min, int Max) : IPayBandEstimateRule
{
    public List<string> Check(PayBandEstimate item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band estimate.</summary>
public sealed record PayBandEstimateValidator(List<IPayBandEstimateRule> Rules)
{
    public List<string> Validate(PayBandEstimate item)
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

    public void RequireValid(PayBandEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandEstimate> items)
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

/// <summary>Functions of pay band estimate rules.</summary>
public static class PayBandEstimateRules
{
    public static PayBandEstimateValidator DefaultPayBandEstimateValidator()
    {
        return new PayBandEstimateValidator(
            new List<IPayBandEstimateRule>
            {
                new PayBandEstimateLabelLengthRule(120),
                new PayBandEstimateSequenceRangeRule(3600, 3600),
                new PayBandEstimateCodePrefixRule("ACM"),
                new PayBandEstimateRequiredOwnerIdRule("pay band estimate needs a value"),
                new PayBandEstimateDurationMinutesRangeRule(32, 86400),
            });
    }
}
