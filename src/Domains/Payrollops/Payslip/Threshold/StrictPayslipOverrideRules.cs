namespace Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>A rule a strict payslip override must pass. Check returns its problems, or none.</summary>
public interface IStrictPayslipOverrideRule
{
    List<string> Check(StrictPayslipOverride item);
}

public sealed record StrictPayslipOverrideCodePrefixRule(string Prefix) : IStrictPayslipOverrideRule
{
    public List<string> Check(StrictPayslipOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayslipOverrideLimitRangeRule(int Min, int Max) : IStrictPayslipOverrideRule
{
    public List<string> Check(StrictPayslipOverride item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayslipOverrideVisibleGuardRule(int Limit) : IStrictPayslipOverrideRule
{
    public List<string> Check(StrictPayslipOverride item)
    {
        if (item.Visible && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayslipOverrideRequiredTimezoneRule(string Message) : IStrictPayslipOverrideRule
{
    public List<string> Check(StrictPayslipOverride item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict payslip override.</summary>
public sealed record StrictPayslipOverrideValidator(List<IStrictPayslipOverrideRule> Rules)
{
    public List<string> Validate(StrictPayslipOverride item)
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

    public void RequireValid(StrictPayslipOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPayslipOverride> items)
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

/// <summary>Functions of strict payslip override rules.</summary>
public static class StrictPayslipOverrideRules
{
    public static StrictPayslipOverrideValidator DefaultStrictPayslipOverrideValidator()
    {
        return new StrictPayslipOverrideValidator(
            new List<IStrictPayslipOverrideRule>
            {
                new StrictPayslipOverrideCodePrefixRule("OPS"),
                new StrictPayslipOverrideLimitRangeRule(120, 3600),
                new StrictPayslipOverrideVisibleGuardRule(64),
                new StrictPayslipOverrideRequiredTimezoneRule("strict payslip override needs an entry"),
            });
    }
}
