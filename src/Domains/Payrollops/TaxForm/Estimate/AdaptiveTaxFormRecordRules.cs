namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>A rule a adaptive tax form record must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTaxFormRecordRule
{
    List<string> Check(AdaptiveTaxFormRecord item);
}

public sealed record AdaptiveTaxFormRecordBatchSizeRangeRule(int Min, int Max) : IAdaptiveTaxFormRecordRule
{
    public List<string> Check(AdaptiveTaxFormRecord item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTaxFormRecordRateRangeRule(double Min, double Max) : IAdaptiveTaxFormRecordRule
{
    public List<string> Check(AdaptiveTaxFormRecord item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTaxFormRecordChannelLengthRule(int MaxLength) : IAdaptiveTaxFormRecordRule
{
    public List<string> Check(AdaptiveTaxFormRecord item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTaxFormRecordAmountRangeRule(double Min, double Max) : IAdaptiveTaxFormRecordRule
{
    public List<string> Check(AdaptiveTaxFormRecord item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive tax form record.</summary>
public sealed record AdaptiveTaxFormRecordValidator(List<IAdaptiveTaxFormRecordRule> Rules)
{
    public List<string> Validate(AdaptiveTaxFormRecord item)
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

    public void RequireValid(AdaptiveTaxFormRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTaxFormRecord> items)
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

/// <summary>Functions of adaptive tax form record rules.</summary>
public static class AdaptiveTaxFormRecordRules
{
    public static AdaptiveTaxFormRecordValidator DefaultAdaptiveTaxFormRecordValidator()
    {
        return new AdaptiveTaxFormRecordValidator(
            new List<IAdaptiveTaxFormRecordRule>
            {
                new AdaptiveTaxFormRecordBatchSizeRangeRule(16, 64),
                new AdaptiveTaxFormRecordRateRangeRule(3.3, 3481.3),
                new AdaptiveTaxFormRecordChannelLengthRule(32),
                new AdaptiveTaxFormRecordAmountRangeRule(8.7, 2160.4),
            });
    }
}
