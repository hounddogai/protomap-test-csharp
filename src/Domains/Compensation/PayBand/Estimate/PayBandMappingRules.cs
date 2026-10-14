namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band mapping must pass. Check returns its problems, or none.</summary>
public interface IPayBandMappingRule
{
    List<string> Check(PayBandMapping item);
}

public sealed record PayBandMappingPhoneLengthRule(int MaxLength) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMappingRequiredCodeRule(string Message) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandMappingRequiredLabelRule(string Message) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandMappingLimitRangeRule(int Min, int Max) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMappingAllowedTitleRule(List<string> Allowed) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMappingCodeLengthRule(int MaxLength) : IPayBandMappingRule
{
    public List<string> Check(PayBandMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band mapping.</summary>
public sealed record PayBandMappingValidator(List<IPayBandMappingRule> Rules)
{
    public List<string> Validate(PayBandMapping item)
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

    public void RequireValid(PayBandMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandMapping> items)
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

/// <summary>Functions of pay band mapping rules.</summary>
public static class PayBandMappingRules
{
    public static PayBandMappingValidator DefaultPayBandMappingValidator()
    {
        return new PayBandMappingValidator(
            new List<IPayBandMappingRule>
            {
                new PayBandMappingPhoneLengthRule(16),
                new PayBandMappingRequiredCodeRule("pay band mapping needs a value"),
                new PayBandMappingRequiredLabelRule("pay band mapping needs a value"),
                new PayBandMappingLimitRangeRule(86400, 86400),
                new PayBandMappingAllowedTitleRule(new List<string> { "pending" }),
                new PayBandMappingCodeLengthRule(80),
            });
    }
}
