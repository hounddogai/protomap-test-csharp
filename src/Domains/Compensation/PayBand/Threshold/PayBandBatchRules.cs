namespace Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>A rule a pay band batch must pass. Check returns its problems, or none.</summary>
public interface IPayBandBatchRule
{
    List<string> Check(PayBandBatch item);
}

public sealed record PayBandBatchRequiredLabelRule(string Message) : IPayBandBatchRule
{
    public List<string> Check(PayBandBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandBatchCodeLengthRule(int MaxLength) : IPayBandBatchRule
{
    public List<string> Check(PayBandBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandBatchRequiredCodeRule(string Message) : IPayBandBatchRule
{
    public List<string> Check(PayBandBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandBatchLabelLengthRule(int MaxLength) : IPayBandBatchRule
{
    public List<string> Check(PayBandBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band batch.</summary>
public sealed record PayBandBatchValidator(List<IPayBandBatchRule> Rules)
{
    public List<string> Validate(PayBandBatch item)
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

    public void RequireValid(PayBandBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandBatch> items)
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

/// <summary>Functions of pay band batch rules.</summary>
public static class PayBandBatchRules
{
    public static PayBandBatchValidator DefaultPayBandBatchValidator()
    {
        return new PayBandBatchValidator(
            new List<IPayBandBatchRule>
            {
                new PayBandBatchRequiredLabelRule("pay band batch needs this field"),
                new PayBandBatchCodeLengthRule(64),
                new PayBandBatchRequiredCodeRule("pay band batch needs this field"),
                new PayBandBatchLabelLengthRule(3600),
            });
    }
}
