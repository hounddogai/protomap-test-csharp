namespace Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>A rule a deduction entry must pass. Check returns its problems, or none.</summary>
public interface IDeductionEntryRule
{
    List<string> Check(DeductionEntry item);
}

public sealed record DeductionEntryCodeLengthRule(int MaxLength) : IDeductionEntryRule
{
    public List<string> Check(DeductionEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionEntryChannelLengthRule(int MaxLength) : IDeductionEntryRule
{
    public List<string> Check(DeductionEntry item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionEntryRequiredChannelRule(string Message) : IDeductionEntryRule
{
    public List<string> Check(DeductionEntry item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionEntryRequiredCountryRule(string Message) : IDeductionEntryRule
{
    public List<string> Check(DeductionEntry item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction entry.</summary>
public sealed record DeductionEntryValidator(List<IDeductionEntryRule> Rules)
{
    public List<string> Validate(DeductionEntry item)
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

    public void RequireValid(DeductionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionEntry> items)
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

/// <summary>Functions of deduction entry rules.</summary>
public static class DeductionEntryRules
{
    public static DeductionEntryValidator DefaultDeductionEntryValidator()
    {
        return new DeductionEntryValidator(
            new List<IDeductionEntryRule>
            {
                new DeductionEntryCodeLengthRule(3600),
                new DeductionEntryChannelLengthRule(32),
                new DeductionEntryRequiredChannelRule("deduction entry needs an entry"),
                new DeductionEntryRequiredCountryRule("deduction entry needs a value"),
            });
    }
}
