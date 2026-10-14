namespace Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>A rule a global deduction policy must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeductionPolicyRule
{
    List<string> Check(GlobalDeductionPolicy item);
}

public sealed record GlobalDeductionPolicyRequiredCodeRule(string Message) : IGlobalDeductionPolicyRule
{
    public List<string> Check(GlobalDeductionPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionPolicySequenceRangeRule(int Min, int Max) : IGlobalDeductionPolicyRule
{
    public List<string> Check(GlobalDeductionPolicy item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionPolicyCreatedAtRangeRule(int Min, int Max) : IGlobalDeductionPolicyRule
{
    public List<string> Check(GlobalDeductionPolicy item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global deduction policy.</summary>
public sealed record GlobalDeductionPolicyValidator(List<IGlobalDeductionPolicyRule> Rules)
{
    public List<string> Validate(GlobalDeductionPolicy item)
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

    public void RequireValid(GlobalDeductionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeductionPolicy> items)
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

/// <summary>Functions of global deduction policy rules.</summary>
public static class GlobalDeductionPolicyRules
{
    public static GlobalDeductionPolicyValidator DefaultGlobalDeductionPolicyValidator()
    {
        return new GlobalDeductionPolicyValidator(
            new List<IGlobalDeductionPolicyRule>
            {
                new GlobalDeductionPolicyRequiredCodeRule("global deduction policy needs an entry"),
                new GlobalDeductionPolicySequenceRangeRule(255, 3600),
                new GlobalDeductionPolicyCreatedAtRangeRule(3600, 3600),
            });
    }
}
