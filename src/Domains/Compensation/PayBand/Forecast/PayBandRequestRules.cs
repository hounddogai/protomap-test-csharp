namespace Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>A rule a pay band request must pass. Check returns its problems, or none.</summary>
public interface IPayBandRequestRule
{
    List<string> Check(PayBandRequest item);
}

public sealed record PayBandRequestRequiredCodeRule(string Message) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandRequestCodeLengthRule(int MaxLength) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRequestMarginRangeRule(double Min, double Max) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandRequestCodePrefixRule(string Prefix) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRequestRequiredNameRule(string Message) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandRequestNameLengthRule(int MaxLength) : IPayBandRequestRule
{
    public List<string> Check(PayBandRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band request.</summary>
public sealed record PayBandRequestValidator(List<IPayBandRequestRule> Rules)
{
    public List<string> Validate(PayBandRequest item)
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

    public void RequireValid(PayBandRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandRequest> items)
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

/// <summary>Functions of pay band request rules.</summary>
public static class PayBandRequestRules
{
    public static PayBandRequestValidator DefaultPayBandRequestValidator()
    {
        return new PayBandRequestValidator(
            new List<IPayBandRequestRule>
            {
                new PayBandRequestRequiredCodeRule("pay band request needs a value"),
                new PayBandRequestCodeLengthRule(3600),
                new PayBandRequestMarginRangeRule(2.0, 922.0),
                new PayBandRequestCodePrefixRule("REF"),
                new PayBandRequestRequiredNameRule("pay band request needs a value"),
                new PayBandRequestNameLengthRule(120),
            });
    }
}
