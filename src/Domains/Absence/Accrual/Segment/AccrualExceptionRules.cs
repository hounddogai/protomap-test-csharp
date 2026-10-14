namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual exception must pass. Check returns its problems, or none.</summary>
public interface IAccrualExceptionRule
{
    List<string> Check(AccrualException item);
}

public sealed record AccrualExceptionCodePrefixRule(string Prefix) : IAccrualExceptionRule
{
    public List<string> Check(AccrualException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualExceptionGraceDaysRangeRule(int Min, int Max) : IAccrualExceptionRule
{
    public List<string> Check(AccrualException item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualExceptionLimitRangeRule(int Min, int Max) : IAccrualExceptionRule
{
    public List<string> Check(AccrualException item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualExceptionAllowedManagerIdRule(List<string> Allowed) : IAccrualExceptionRule
{
    public List<string> Check(AccrualException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AccrualExceptionSalaryBandLengthRule(int MaxLength) : IAccrualExceptionRule
{
    public List<string> Check(AccrualException item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual exception.</summary>
public sealed record AccrualExceptionValidator(List<IAccrualExceptionRule> Rules)
{
    public List<string> Validate(AccrualException item)
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

    public void RequireValid(AccrualException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualException> items)
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

/// <summary>Functions of accrual exception rules.</summary>
public static class AccrualExceptionRules
{
    public static AccrualExceptionValidator DefaultAccrualExceptionValidator()
    {
        return new AccrualExceptionValidator(
            new List<IAccrualExceptionRule>
            {
                new AccrualExceptionCodePrefixRule("ACM"),
                new AccrualExceptionGraceDaysRangeRule(16, 255),
                new AccrualExceptionLimitRangeRule(32, 80),
                new AccrualExceptionAllowedManagerIdRule(new List<string> { "pending", "lenient", "primary" }),
                new AccrualExceptionSalaryBandLengthRule(120),
            });
    }
}
