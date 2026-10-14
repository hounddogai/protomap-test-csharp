namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>A rule a fallback payslip checkpoint must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayslipCheckpointRule
{
    List<string> Check(FallbackPayslipCheckpoint item);
}

public sealed record FallbackPayslipCheckpointCodePrefixRule(string Prefix) : IFallbackPayslipCheckpointRule
{
    public List<string> Check(FallbackPayslipCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipCheckpointCurrencyLengthRule(int MaxLength) : IFallbackPayslipCheckpointRule
{
    public List<string> Check(FallbackPayslipCheckpoint item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipCheckpointAmountRangeRule(double Min, double Max) : IFallbackPayslipCheckpointRule
{
    public List<string> Check(FallbackPayslipCheckpoint item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipCheckpointRequiredNameRule(string Message) : IFallbackPayslipCheckpointRule
{
    public List<string> Check(FallbackPayslipCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback payslip checkpoint.</summary>
public sealed record FallbackPayslipCheckpointValidator(List<IFallbackPayslipCheckpointRule> Rules)
{
    public List<string> Validate(FallbackPayslipCheckpoint item)
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

    public void RequireValid(FallbackPayslipCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayslipCheckpoint> items)
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

/// <summary>Functions of fallback payslip checkpoint rules.</summary>
public static class FallbackPayslipCheckpointRules
{
    public static FallbackPayslipCheckpointValidator DefaultFallbackPayslipCheckpointValidator()
    {
        return new FallbackPayslipCheckpointValidator(
            new List<IFallbackPayslipCheckpointRule>
            {
                new FallbackPayslipCheckpointCodePrefixRule("OPS"),
                new FallbackPayslipCheckpointCurrencyLengthRule(16),
                new FallbackPayslipCheckpointAmountRangeRule(2.1, 1722.5),
                new FallbackPayslipCheckpointRequiredNameRule("fallback payslip checkpoint needs a value"),
            });
    }
}
