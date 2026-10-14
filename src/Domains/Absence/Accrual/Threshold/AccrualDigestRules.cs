namespace Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>A rule a accrual digest must pass. Check returns its problems, or none.</summary>
public interface IAccrualDigestRule
{
    List<string> Check(AccrualDigest item);
}

public sealed record AccrualDigestFloorValueRangeRule(double Min, double Max) : IAccrualDigestRule
{
    public List<string> Check(AccrualDigest item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDigestLabelLengthRule(int MaxLength) : IAccrualDigestRule
{
    public List<string> Check(AccrualDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDigestRequiredSourceRule(string Message) : IAccrualDigestRule
{
    public List<string> Check(AccrualDigest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual digest.</summary>
public sealed record AccrualDigestValidator(List<IAccrualDigestRule> Rules)
{
    public List<string> Validate(AccrualDigest item)
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

    public void RequireValid(AccrualDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualDigest> items)
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

/// <summary>Functions of accrual digest rules.</summary>
public static class AccrualDigestRules
{
    public static AccrualDigestValidator DefaultAccrualDigestValidator()
    {
        return new AccrualDigestValidator(
            new List<IAccrualDigestRule>
            {
                new AccrualDigestFloorValueRangeRule(0.8, 4926.7),
                new AccrualDigestLabelLengthRule(3600),
                new AccrualDigestRequiredSourceRule("accrual digest needs this field"),
            });
    }
}
