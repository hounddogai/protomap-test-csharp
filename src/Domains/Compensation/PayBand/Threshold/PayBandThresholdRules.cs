namespace Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>A rule a pay band threshold must pass. Check returns its problems, or none.</summary>
public interface IPayBandThresholdRule
{
    List<string> Check(PayBandThreshold item);
}

public sealed record PayBandThresholdLabelLengthRule(int MaxLength) : IPayBandThresholdRule
{
    public List<string> Check(PayBandThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandThresholdLeaveDaysRangeRule(int Min, int Max) : IPayBandThresholdRule
{
    public List<string> Check(PayBandThreshold item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandThresholdTenureMonthsRangeRule(int Min, int Max) : IPayBandThresholdRule
{
    public List<string> Check(PayBandThreshold item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandThresholdCodeLengthRule(int MaxLength) : IPayBandThresholdRule
{
    public List<string> Check(PayBandThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band threshold.</summary>
public sealed record PayBandThresholdValidator(List<IPayBandThresholdRule> Rules)
{
    public List<string> Validate(PayBandThreshold item)
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

    public void RequireValid(PayBandThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandThreshold> items)
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

/// <summary>Functions of pay band threshold rules.</summary>
public static class PayBandThresholdRules
{
    public static PayBandThresholdValidator DefaultPayBandThresholdValidator()
    {
        return new PayBandThresholdValidator(
            new List<IPayBandThresholdRule>
            {
                new PayBandThresholdLabelLengthRule(86400),
                new PayBandThresholdLeaveDaysRangeRule(80, 120),
                new PayBandThresholdTenureMonthsRangeRule(64, 86400),
                new PayBandThresholdCodeLengthRule(32),
            });
    }
}
