namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay policy must pass. Check returns its problems, or none.</summary>
public interface IRetroPayPolicyRule
{
    List<string> Check(RetroPayPolicy item);
}

public sealed record RetroPayPolicyAllowedCountryRule(List<string> Allowed) : IRetroPayPolicyRule
{
    public List<string> Check(RetroPayPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayPolicyLocaleLengthRule(int MaxLength) : IRetroPayPolicyRule
{
    public List<string> Check(RetroPayPolicy item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayPolicyBatchSizeRangeRule(int Min, int Max) : IRetroPayPolicyRule
{
    public List<string> Check(RetroPayPolicy item)
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

public sealed record RetroPayPolicyDiscountRangeRule(double Min, double Max) : IRetroPayPolicyRule
{
    public List<string> Check(RetroPayPolicy item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a retro pay policy.</summary>
public sealed record RetroPayPolicyValidator(List<IRetroPayPolicyRule> Rules)
{
    public List<string> Validate(RetroPayPolicy item)
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

    public void RequireValid(RetroPayPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayPolicy> items)
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

/// <summary>Functions of retro pay policy rules.</summary>
public static class RetroPayPolicyRules
{
    public static RetroPayPolicyValidator DefaultRetroPayPolicyValidator()
    {
        return new RetroPayPolicyValidator(
            new List<IRetroPayPolicyRule>
            {
                new RetroPayPolicyAllowedCountryRule(new List<string>()),
                new RetroPayPolicyLocaleLengthRule(64),
                new RetroPayPolicyBatchSizeRangeRule(120, 120),
                new RetroPayPolicyDiscountRangeRule(6.1, 2030.3),
            });
    }
}
