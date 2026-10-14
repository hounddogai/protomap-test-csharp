namespace Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>A rule a pay band bundle must pass. Check returns its problems, or none.</summary>
public interface IPayBandBundleRule
{
    List<string> Check(PayBandBundle item);
}

public sealed record PayBandBundleEndsAtRangeRule(int Min, int Max) : IPayBandBundleRule
{
    public List<string> Check(PayBandBundle item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandBundleEmployeeIdLengthRule(int MaxLength) : IPayBandBundleRule
{
    public List<string> Check(PayBandBundle item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandBundleNameLengthRule(int MaxLength) : IPayBandBundleRule
{
    public List<string> Check(PayBandBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandBundleRequiredCodeRule(string Message) : IPayBandBundleRule
{
    public List<string> Check(PayBandBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandBundleCodeLengthRule(int MaxLength) : IPayBandBundleRule
{
    public List<string> Check(PayBandBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band bundle.</summary>
public sealed record PayBandBundleValidator(List<IPayBandBundleRule> Rules)
{
    public List<string> Validate(PayBandBundle item)
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

    public void RequireValid(PayBandBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandBundle> items)
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

/// <summary>Functions of pay band bundle rules.</summary>
public static class PayBandBundleRules
{
    public static PayBandBundleValidator DefaultPayBandBundleValidator()
    {
        return new PayBandBundleValidator(
            new List<IPayBandBundleRule>
            {
                new PayBandBundleEndsAtRangeRule(64, 1000),
                new PayBandBundleEmployeeIdLengthRule(120),
                new PayBandBundleNameLengthRule(80),
                new PayBandBundleRequiredCodeRule("pay band bundle needs this field"),
                new PayBandBundleCodeLengthRule(16),
            });
    }
}
