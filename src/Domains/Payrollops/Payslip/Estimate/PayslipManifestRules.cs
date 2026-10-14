namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>A rule a payslip manifest must pass. Check returns its problems, or none.</summary>
public interface IPayslipManifestRule
{
    List<string> Check(PayslipManifest item);
}

public sealed record PayslipManifestTitleLengthRule(int MaxLength) : IPayslipManifestRule
{
    public List<string> Check(PayslipManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipManifestStartsAtRangeRule(int Min, int Max) : IPayslipManifestRule
{
    public List<string> Check(PayslipManifest item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipManifestSourceLengthRule(int MaxLength) : IPayslipManifestRule
{
    public List<string> Check(PayslipManifest item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipManifestRequiredTitleRule(string Message) : IPayslipManifestRule
{
    public List<string> Check(PayslipManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipManifestRequiredCodeRule(string Message) : IPayslipManifestRule
{
    public List<string> Check(PayslipManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip manifest.</summary>
public sealed record PayslipManifestValidator(List<IPayslipManifestRule> Rules)
{
    public List<string> Validate(PayslipManifest item)
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

    public void RequireValid(PayslipManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipManifest> items)
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

/// <summary>Functions of payslip manifest rules.</summary>
public static class PayslipManifestRules
{
    public static PayslipManifestValidator DefaultPayslipManifestValidator()
    {
        return new PayslipManifestValidator(
            new List<IPayslipManifestRule>
            {
                new PayslipManifestTitleLengthRule(1000),
                new PayslipManifestStartsAtRangeRule(80, 120),
                new PayslipManifestSourceLengthRule(3600),
                new PayslipManifestRequiredTitleRule("payslip manifest needs an entry"),
                new PayslipManifestRequiredCodeRule("payslip manifest needs this field"),
            });
    }
}
