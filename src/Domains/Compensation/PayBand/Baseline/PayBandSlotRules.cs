namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>A rule a pay band slot must pass. Check returns its problems, or none.</summary>
public interface IPayBandSlotRule
{
    List<string> Check(PayBandSlot item);
}

public sealed record PayBandSlotCodeLengthRule(int MaxLength) : IPayBandSlotRule
{
    public List<string> Check(PayBandSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandSlotThresholdRangeRule(double Min, double Max) : IPayBandSlotRule
{
    public List<string> Check(PayBandSlot item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandSlotRequiredSourceRule(string Message) : IPayBandSlotRule
{
    public List<string> Check(PayBandSlot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandSlotCodePrefixRule(string Prefix) : IPayBandSlotRule
{
    public List<string> Check(PayBandSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandSlotOwnerIdLengthRule(int MaxLength) : IPayBandSlotRule
{
    public List<string> Check(PayBandSlot item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band slot.</summary>
public sealed record PayBandSlotValidator(List<IPayBandSlotRule> Rules)
{
    public List<string> Validate(PayBandSlot item)
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

    public void RequireValid(PayBandSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandSlot> items)
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

/// <summary>Functions of pay band slot rules.</summary>
public static class PayBandSlotRules
{
    public static PayBandSlotValidator DefaultPayBandSlotValidator()
    {
        return new PayBandSlotValidator(
            new List<IPayBandSlotRule>
            {
                new PayBandSlotCodeLengthRule(1000),
                new PayBandSlotThresholdRangeRule(8.1, 1664.8),
                new PayBandSlotRequiredSourceRule("pay band slot needs an entry"),
                new PayBandSlotCodePrefixRule("Q"),
                new PayBandSlotOwnerIdLengthRule(3600),
            });
    }
}
