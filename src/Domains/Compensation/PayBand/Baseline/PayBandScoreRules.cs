namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>A rule a pay band score must pass. Check returns its problems, or none.</summary>
public interface IPayBandScoreRule
{
    List<string> Check(PayBandScore item);
}

public sealed record PayBandScoreRequiredLabelRule(string Message) : IPayBandScoreRule
{
    public List<string> Check(PayBandScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandScoreMinimumRangeRule(double Min, double Max) : IPayBandScoreRule
{
    public List<string> Check(PayBandScore item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandScoreTitleLengthRule(int MaxLength) : IPayBandScoreRule
{
    public List<string> Check(PayBandScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandScoreLabelLengthRule(int MaxLength) : IPayBandScoreRule
{
    public List<string> Check(PayBandScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band score.</summary>
public sealed record PayBandScoreValidator(List<IPayBandScoreRule> Rules)
{
    public List<string> Validate(PayBandScore item)
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

    public void RequireValid(PayBandScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandScore> items)
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

/// <summary>Functions of pay band score rules.</summary>
public static class PayBandScoreRules
{
    public static PayBandScoreValidator DefaultPayBandScoreValidator()
    {
        return new PayBandScoreValidator(
            new List<IPayBandScoreRule>
            {
                new PayBandScoreRequiredLabelRule("pay band score needs a value"),
                new PayBandScoreMinimumRangeRule(7.3, 3423.0),
                new PayBandScoreTitleLengthRule(64),
                new PayBandScoreLabelLengthRule(120),
            });
    }
}
