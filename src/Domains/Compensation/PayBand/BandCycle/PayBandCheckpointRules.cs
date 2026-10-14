namespace Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>A rule a pay band checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPayBandCheckpointRule
{
    List<string> Check(PayBandCheckpoint item);
}

public sealed record PayBandCheckpointCodeLengthRule(int MaxLength) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandCheckpointRequiredJobCodeRule(string Message) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandCheckpointRequiredSegmentRule(string Message) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandCheckpointBudgetRangeRule(double Min, double Max) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandCheckpointRatioRangeRule(double Min, double Max) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandCheckpointRequiredCodeRule(string Message) : IPayBandCheckpointRule
{
    public List<string> Check(PayBandCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band checkpoint.</summary>
public sealed record PayBandCheckpointValidator(List<IPayBandCheckpointRule> Rules)
{
    public List<string> Validate(PayBandCheckpoint item)
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

    public void RequireValid(PayBandCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandCheckpoint> items)
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

/// <summary>Functions of pay band checkpoint rules.</summary>
public static class PayBandCheckpointRules
{
    public static PayBandCheckpointValidator DefaultPayBandCheckpointValidator()
    {
        return new PayBandCheckpointValidator(
            new List<IPayBandCheckpointRule>
            {
                new PayBandCheckpointCodeLengthRule(32),
                new PayBandCheckpointRequiredJobCodeRule("pay band checkpoint needs this field"),
                new PayBandCheckpointRequiredSegmentRule("pay band checkpoint needs this field"),
                new PayBandCheckpointBudgetRangeRule(7.3, 1782.0),
                new PayBandCheckpointRatioRangeRule(6.4, 4624.4),
                new PayBandCheckpointRequiredCodeRule("pay band checkpoint needs an entry"),
            });
    }
}
