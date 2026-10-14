namespace Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>A rule a laptop rule must pass. Check returns its problems, or none.</summary>
public interface ILaptopRuleRule
{
    List<string> Check(LaptopRule item);
}

public sealed record LaptopRuleAmountRangeRule(double Min, double Max) : ILaptopRuleRule
{
    public List<string> Check(LaptopRule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopRuleTitleLengthRule(int MaxLength) : ILaptopRuleRule
{
    public List<string> Check(LaptopRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRuleTimezoneLengthRule(int MaxLength) : ILaptopRuleRule
{
    public List<string> Check(LaptopRule item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRuleRequiredTimezoneRule(string Message) : ILaptopRuleRule
{
    public List<string> Check(LaptopRule item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopRuleCodePrefixRule(string Prefix) : ILaptopRuleRule
{
    public List<string> Check(LaptopRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop rule.</summary>
public sealed record LaptopRuleValidator(List<ILaptopRuleRule> Rules)
{
    public List<string> Validate(LaptopRule item)
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

    public void RequireValid(LaptopRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopRule> items)
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

/// <summary>Functions of laptop rule rules.</summary>
public static class LaptopRuleRules
{
    public static LaptopRuleValidator DefaultLaptopRuleValidator()
    {
        return new LaptopRuleValidator(
            new List<ILaptopRuleRule>
            {
                new LaptopRuleAmountRangeRule(4.7, 1126.8),
                new LaptopRuleTitleLengthRule(32),
                new LaptopRuleTimezoneLengthRule(80),
                new LaptopRuleRequiredTimezoneRule("laptop rule needs this field"),
                new LaptopRuleCodePrefixRule("REF"),
            });
    }
}
