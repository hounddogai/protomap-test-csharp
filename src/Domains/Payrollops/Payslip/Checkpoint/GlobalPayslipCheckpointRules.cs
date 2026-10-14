namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global payslip checkpoint must pass. Check returns its problems, or none.</summary>
public interface IGlobalPayslipCheckpointRule
{
    List<string> Check(GlobalPayslipCheckpoint item);
}

public sealed record GlobalPayslipCheckpointCodePrefixRule(string Prefix) : IGlobalPayslipCheckpointRule
{
    public List<string> Check(GlobalPayslipCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayslipCheckpointNameLengthRule(int MaxLength) : IGlobalPayslipCheckpointRule
{
    public List<string> Check(GlobalPayslipCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayslipCheckpointAllowedCountryRule(List<string> Allowed) : IGlobalPayslipCheckpointRule
{
    public List<string> Check(GlobalPayslipCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayslipCheckpointAmountRangeRule(double Min, double Max) : IGlobalPayslipCheckpointRule
{
    public List<string> Check(GlobalPayslipCheckpoint item)
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

public sealed record GlobalPayslipCheckpointCountryLengthRule(int MaxLength) : IGlobalPayslipCheckpointRule
{
    public List<string> Check(GlobalPayslipCheckpoint item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global payslip checkpoint.</summary>
public sealed record GlobalPayslipCheckpointValidator(List<IGlobalPayslipCheckpointRule> Rules)
{
    public List<string> Validate(GlobalPayslipCheckpoint item)
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

    public void RequireValid(GlobalPayslipCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPayslipCheckpoint> items)
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

/// <summary>Functions of global payslip checkpoint rules.</summary>
public static class GlobalPayslipCheckpointRules
{
    public static GlobalPayslipCheckpointValidator DefaultGlobalPayslipCheckpointValidator()
    {
        return new GlobalPayslipCheckpointValidator(
            new List<IGlobalPayslipCheckpointRule>
            {
                new GlobalPayslipCheckpointCodePrefixRule("ACM"),
                new GlobalPayslipCheckpointNameLengthRule(120),
                new GlobalPayslipCheckpointAllowedCountryRule(new List<string> { "manual" }),
                new GlobalPayslipCheckpointAmountRangeRule(1.2, 3108.1),
                new GlobalPayslipCheckpointCountryLengthRule(64),
            });
    }
}
