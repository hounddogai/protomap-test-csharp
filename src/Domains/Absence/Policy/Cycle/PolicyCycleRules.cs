namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy cycle must pass. Check returns its problems, or none.</summary>
public interface IPolicyCycleRule
{
    List<string> Check(PolicyCycle item);
}

public sealed record PolicyCycleTitleLengthRule(int MaxLength) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyCycleCodeLengthRule(int MaxLength) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyCycleAllowedTitleRule(List<string> Allowed) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PolicyCycleBudgetRangeRule(double Min, double Max) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyCycleCodePrefixRule(string Prefix) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyCycleRequiredCodeRule(string Message) : IPolicyCycleRule
{
    public List<string> Check(PolicyCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy cycle.</summary>
public sealed record PolicyCycleValidator(List<IPolicyCycleRule> Rules)
{
    public List<string> Validate(PolicyCycle item)
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

    public void RequireValid(PolicyCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyCycle> items)
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

/// <summary>Functions of policy cycle rules.</summary>
public static class PolicyCycleRules
{
    public static PolicyCycleValidator DefaultPolicyCycleValidator()
    {
        return new PolicyCycleValidator(
            new List<IPolicyCycleRule>
            {
                new PolicyCycleTitleLengthRule(255),
                new PolicyCycleCodeLengthRule(16),
                new PolicyCycleAllowedTitleRule(new List<string> { "weighted" }),
                new PolicyCycleBudgetRangeRule(2.3, 1510.7),
                new PolicyCycleCodePrefixRule("REF"),
                new PolicyCycleRequiredCodeRule("policy cycle needs an entry"),
            });
    }
}
