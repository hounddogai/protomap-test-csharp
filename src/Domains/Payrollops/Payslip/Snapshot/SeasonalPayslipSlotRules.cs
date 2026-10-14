namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal payslip slot must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayslipSlotRule
{
    List<string> Check(SeasonalPayslipSlot item);
}

public sealed record SeasonalPayslipSlotRequiredCodeRule(string Message) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSlotCodePrefixRule(string Prefix) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSlotCurrencyLengthRule(int MaxLength) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSlotRequiredNameRule(string Message) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSlotAllowedUnitRule(List<string> Allowed) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSlotActiveGuardRule(double Limit) : ISeasonalPayslipSlotRule
{
    public List<string> Check(SeasonalPayslipSlot item)
    {
        if (item.Active && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal payslip slot.</summary>
public sealed record SeasonalPayslipSlotValidator(List<ISeasonalPayslipSlotRule> Rules)
{
    public List<string> Validate(SeasonalPayslipSlot item)
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

    public void RequireValid(SeasonalPayslipSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayslipSlot> items)
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

/// <summary>Functions of seasonal payslip slot rules.</summary>
public static class SeasonalPayslipSlotRules
{
    public static SeasonalPayslipSlotValidator DefaultSeasonalPayslipSlotValidator()
    {
        return new SeasonalPayslipSlotValidator(
            new List<ISeasonalPayslipSlotRule>
            {
                new SeasonalPayslipSlotRequiredCodeRule("seasonal payslip slot needs this field"),
                new SeasonalPayslipSlotCodePrefixRule("OPS"),
                new SeasonalPayslipSlotCurrencyLengthRule(255),
                new SeasonalPayslipSlotRequiredNameRule("seasonal payslip slot needs an entry"),
                new SeasonalPayslipSlotAllowedUnitRule(new List<string> { "fallback", "pending" }),
                new SeasonalPayslipSlotActiveGuardRule(3.9),
            });
    }
}
