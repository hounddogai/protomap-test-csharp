namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>A rule a pay run rule must pass. Check returns its problems, or none.</summary>
public interface IPayRunRuleRule
{
    List<string> Check(PayRunRule item);
}

public sealed record PayRunRuleRequiredCountryRule(string Message) : IPayRunRuleRule
{
    public List<string> Check(PayRunRule item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunRuleRequiredEmployeeIdRule(string Message) : IPayRunRuleRule
{
    public List<string> Check(PayRunRule item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunRuleSequenceRangeRule(int Min, int Max) : IPayRunRuleRule
{
    public List<string> Check(PayRunRule item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunRuleEndsAtRangeRule(int Min, int Max) : IPayRunRuleRule
{
    public List<string> Check(PayRunRule item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run rule.</summary>
public sealed record PayRunRuleValidator(List<IPayRunRuleRule> Rules)
{
    public List<string> Validate(PayRunRule item)
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

    public void RequireValid(PayRunRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunRule> items)
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

/// <summary>Functions of pay run rule rules.</summary>
public static class PayRunRuleRules
{
    public static PayRunRuleValidator DefaultPayRunRuleValidator()
    {
        return new PayRunRuleValidator(
            new List<IPayRunRuleRule>
            {
                new PayRunRuleRequiredCountryRule("pay run rule needs a value"),
                new PayRunRuleRequiredEmployeeIdRule("pay run rule needs this field"),
                new PayRunRuleSequenceRangeRule(120, 3600),
                new PayRunRuleEndsAtRangeRule(64, 120),
            });
    }
}
