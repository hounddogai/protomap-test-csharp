namespace Acme.Workforce.Payrollops.Payslip.Profile;

/// <summary>A rule a payslip profile must pass. Check returns its problems, or none.</summary>
public interface IPayslipProfileRule
{
    List<string> Check(PayslipProfile item);
}

public sealed record PayslipProfileScoreRangeRule(double Min, double Max) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProfilePinnedGuardRule(double Limit) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        if (item.Pinned && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProfileCodePrefixRule(string Prefix) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProfileTitleLengthRule(int MaxLength) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProfileCodeLengthRule(int MaxLength) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProfileRequiredTitleRule(string Message) : IPayslipProfileRule
{
    public List<string> Check(PayslipProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip profile.</summary>
public sealed record PayslipProfileValidator(List<IPayslipProfileRule> Rules)
{
    public List<string> Validate(PayslipProfile item)
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

    public void RequireValid(PayslipProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipProfile> items)
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

/// <summary>Functions of payslip profile rules.</summary>
public static class PayslipProfileRules
{
    public static PayslipProfileValidator DefaultPayslipProfileValidator()
    {
        return new PayslipProfileValidator(
            new List<IPayslipProfileRule>
            {
                new PayslipProfileScoreRangeRule(5.8, 850.1),
                new PayslipProfilePinnedGuardRule(10.0),
                new PayslipProfileCodePrefixRule("OPS"),
                new PayslipProfileTitleLengthRule(86400),
                new PayslipProfileCodeLengthRule(80),
                new PayslipProfileRequiredTitleRule("payslip profile needs an entry"),
            });
    }
}
