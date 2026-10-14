namespace Acme.Workforce.Absence.Balance.Draft;

/// <summary>A rule a lenient balance plan must pass. Check returns its problems, or none.</summary>
public interface ILenientBalancePlanRule
{
    List<string> Check(LenientBalancePlan item);
}

public sealed record LenientBalancePlanTenureMonthsRangeRule(int Min, int Max) : ILenientBalancePlanRule
{
    public List<string> Check(LenientBalancePlan item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalancePlanRequiredCodeRule(string Message) : ILenientBalancePlanRule
{
    public List<string> Check(LenientBalancePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalancePlanCodePrefixRule(string Prefix) : ILenientBalancePlanRule
{
    public List<string> Check(LenientBalancePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalancePlanRequiredSalaryBandRule(string Message) : ILenientBalancePlanRule
{
    public List<string> Check(LenientBalancePlan item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient balance plan.</summary>
public sealed record LenientBalancePlanValidator(List<ILenientBalancePlanRule> Rules)
{
    public List<string> Validate(LenientBalancePlan item)
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

    public void RequireValid(LenientBalancePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBalancePlan> items)
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

/// <summary>Functions of lenient balance plan rules.</summary>
public static class LenientBalancePlanRules
{
    public static LenientBalancePlanValidator DefaultLenientBalancePlanValidator()
    {
        return new LenientBalancePlanValidator(
            new List<ILenientBalancePlanRule>
            {
                new LenientBalancePlanTenureMonthsRangeRule(16, 3600),
                new LenientBalancePlanRequiredCodeRule("lenient balance plan needs an entry"),
                new LenientBalancePlanCodePrefixRule("OPS"),
                new LenientBalancePlanRequiredSalaryBandRule("lenient balance plan needs an entry"),
            });
    }
}
