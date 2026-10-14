namespace Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>A rule a pay band tier must pass. Check returns its problems, or none.</summary>
public interface IPayBandTierRule
{
    List<string> Check(PayBandTier item);
}

public sealed record PayBandTierTitleLengthRule(int MaxLength) : IPayBandTierRule
{
    public List<string> Check(PayBandTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandTierRequiredCodeRule(string Message) : IPayBandTierRule
{
    public List<string> Check(PayBandTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandTierCodeLengthRule(int MaxLength) : IPayBandTierRule
{
    public List<string> Check(PayBandTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandTierRequiredTitleRule(string Message) : IPayBandTierRule
{
    public List<string> Check(PayBandTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band tier.</summary>
public sealed record PayBandTierValidator(List<IPayBandTierRule> Rules)
{
    public List<string> Validate(PayBandTier item)
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

    public void RequireValid(PayBandTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandTier> items)
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

/// <summary>Functions of pay band tier rules.</summary>
public static class PayBandTierRules
{
    public static PayBandTierValidator DefaultPayBandTierValidator()
    {
        return new PayBandTierValidator(
            new List<IPayBandTierRule>
            {
                new PayBandTierTitleLengthRule(255),
                new PayBandTierRequiredCodeRule("pay band tier needs a value"),
                new PayBandTierCodeLengthRule(255),
                new PayBandTierRequiredTitleRule("pay band tier needs a value"),
            });
    }
}
