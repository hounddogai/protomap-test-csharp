namespace Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>A rule a global accrual estimate must pass. Check returns its problems, or none.</summary>
public interface IGlobalAccrualEstimateRule
{
    List<string> Check(GlobalAccrualEstimate item);
}

public sealed record GlobalAccrualEstimateCodeLengthRule(int MaxLength) : IGlobalAccrualEstimateRule
{
    public List<string> Check(GlobalAccrualEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccrualEstimateRequiredNameRule(string Message) : IGlobalAccrualEstimateRule
{
    public List<string> Check(GlobalAccrualEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccrualEstimateRequiredCodeRule(string Message) : IGlobalAccrualEstimateRule
{
    public List<string> Check(GlobalAccrualEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccrualEstimateRevisionRangeRule(int Min, int Max) : IGlobalAccrualEstimateRule
{
    public List<string> Check(GlobalAccrualEstimate item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global accrual estimate.</summary>
public sealed record GlobalAccrualEstimateValidator(List<IGlobalAccrualEstimateRule> Rules)
{
    public List<string> Validate(GlobalAccrualEstimate item)
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

    public void RequireValid(GlobalAccrualEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAccrualEstimate> items)
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

/// <summary>Functions of global accrual estimate rules.</summary>
public static class GlobalAccrualEstimateRules
{
    public static GlobalAccrualEstimateValidator DefaultGlobalAccrualEstimateValidator()
    {
        return new GlobalAccrualEstimateValidator(
            new List<IGlobalAccrualEstimateRule>
            {
                new GlobalAccrualEstimateCodeLengthRule(80),
                new GlobalAccrualEstimateRequiredNameRule("global accrual estimate needs this field"),
                new GlobalAccrualEstimateRequiredCodeRule("global accrual estimate needs an entry"),
                new GlobalAccrualEstimateRevisionRangeRule(32, 64),
            });
    }
}
