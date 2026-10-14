namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band group must pass. Check returns its problems, or none.</summary>
public interface IPayBandGroupRule
{
    List<string> Check(PayBandGroup item);
}

public sealed record PayBandGroupCodePrefixRule(string Prefix) : IPayBandGroupRule
{
    public List<string> Check(PayBandGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandGroupCodeLengthRule(int MaxLength) : IPayBandGroupRule
{
    public List<string> Check(PayBandGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandGroupAllowedManagerIdRule(List<string> Allowed) : IPayBandGroupRule
{
    public List<string> Check(PayBandGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is missing" };
        }
        return new List<string>();
    }
}

public sealed record PayBandGroupRequiredLabelRule(string Message) : IPayBandGroupRule
{
    public List<string> Check(PayBandGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandGroupTenureMonthsRangeRule(int Min, int Max) : IPayBandGroupRule
{
    public List<string> Check(PayBandGroup item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band group.</summary>
public sealed record PayBandGroupValidator(List<IPayBandGroupRule> Rules)
{
    public List<string> Validate(PayBandGroup item)
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

    public void RequireValid(PayBandGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandGroup> items)
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

/// <summary>Functions of pay band group rules.</summary>
public static class PayBandGroupRules
{
    public static PayBandGroupValidator DefaultPayBandGroupValidator()
    {
        return new PayBandGroupValidator(
            new List<IPayBandGroupRule>
            {
                new PayBandGroupCodePrefixRule("OPS"),
                new PayBandGroupCodeLengthRule(1000),
                new PayBandGroupAllowedManagerIdRule(new List<string> { "primary", "batched" }),
                new PayBandGroupRequiredLabelRule("pay band group needs this field"),
                new PayBandGroupTenureMonthsRangeRule(16, 120),
            });
    }
}
