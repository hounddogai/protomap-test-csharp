namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>A rule a license seat cycle must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatCycleRule
{
    List<string> Check(LicenseSeatCycle item);
}

public sealed record LicenseSeatCycleNameLengthRule(int MaxLength) : ILicenseSeatCycleRule
{
    public List<string> Check(LicenseSeatCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatCycleBudgetRangeRule(double Min, double Max) : ILicenseSeatCycleRule
{
    public List<string> Check(LicenseSeatCycle item)
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

public sealed record LicenseSeatCycleRequiredCodeRule(string Message) : ILicenseSeatCycleRule
{
    public List<string> Check(LicenseSeatCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatCycleReviewerLengthRule(int MaxLength) : ILicenseSeatCycleRule
{
    public List<string> Check(LicenseSeatCycle item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat cycle.</summary>
public sealed record LicenseSeatCycleValidator(List<ILicenseSeatCycleRule> Rules)
{
    public List<string> Validate(LicenseSeatCycle item)
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

    public void RequireValid(LicenseSeatCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatCycle> items)
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

/// <summary>Functions of license seat cycle rules.</summary>
public static class LicenseSeatCycleRules
{
    public static LicenseSeatCycleValidator DefaultLicenseSeatCycleValidator()
    {
        return new LicenseSeatCycleValidator(
            new List<ILicenseSeatCycleRule>
            {
                new LicenseSeatCycleNameLengthRule(16),
                new LicenseSeatCycleBudgetRangeRule(6.3, 3364.7),
                new LicenseSeatCycleRequiredCodeRule("license seat cycle needs a value"),
                new LicenseSeatCycleReviewerLengthRule(3600),
            });
    }
}
