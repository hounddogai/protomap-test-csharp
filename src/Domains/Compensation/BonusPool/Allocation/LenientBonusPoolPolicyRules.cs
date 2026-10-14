namespace Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>A rule a lenient bonus pool policy must pass. Check returns its problems, or none.</summary>
public interface ILenientBonusPoolPolicyRule
{
    List<string> Check(LenientBonusPoolPolicy item);
}

public sealed record LenientBonusPoolPolicyCodeLengthRule(int MaxLength) : ILenientBonusPoolPolicyRule
{
    public List<string> Check(LenientBonusPoolPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolPolicyRequiredNameRule(string Message) : ILenientBonusPoolPolicyRule
{
    public List<string> Check(LenientBonusPoolPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolPolicyLocationLengthRule(int MaxLength) : ILenientBonusPoolPolicyRule
{
    public List<string> Check(LenientBonusPoolPolicy item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolPolicyRequiredCodeRule(string Message) : ILenientBonusPoolPolicyRule
{
    public List<string> Check(LenientBonusPoolPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolPolicyHoursRangeRule(double Min, double Max) : ILenientBonusPoolPolicyRule
{
    public List<string> Check(LenientBonusPoolPolicy item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient bonus pool policy.</summary>
public sealed record LenientBonusPoolPolicyValidator(List<ILenientBonusPoolPolicyRule> Rules)
{
    public List<string> Validate(LenientBonusPoolPolicy item)
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

    public void RequireValid(LenientBonusPoolPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBonusPoolPolicy> items)
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

/// <summary>Functions of lenient bonus pool policy rules.</summary>
public static class LenientBonusPoolPolicyRules
{
    public static LenientBonusPoolPolicyValidator DefaultLenientBonusPoolPolicyValidator()
    {
        return new LenientBonusPoolPolicyValidator(
            new List<ILenientBonusPoolPolicyRule>
            {
                new LenientBonusPoolPolicyCodeLengthRule(255),
                new LenientBonusPoolPolicyRequiredNameRule("lenient bonus pool policy needs a value"),
                new LenientBonusPoolPolicyLocationLengthRule(1000),
                new LenientBonusPoolPolicyRequiredCodeRule("lenient bonus pool policy needs this field"),
                new LenientBonusPoolPolicyHoursRangeRule(6.6, 1766.7),
            });
    }
}
