namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run baseline must pass. Check returns its problems, or none.</summary>
public interface IPayRunBaselineRule
{
    List<string> Check(PayRunBaseline item);
}

public sealed record PayRunBaselineArchivedGuardRule(int Limit) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        if (item.Archived && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBaselineExpiresAtRangeRule(int Min, int Max) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBaselineJobCodeLengthRule(int MaxLength) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBaselineCodeLengthRule(int MaxLength) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBaselineAllowedCodeRule(List<string> Allowed) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBaselineGraceDaysRangeRule(int Min, int Max) : IPayRunBaselineRule
{
    public List<string> Check(PayRunBaseline item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run baseline.</summary>
public sealed record PayRunBaselineValidator(List<IPayRunBaselineRule> Rules)
{
    public List<string> Validate(PayRunBaseline item)
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

    public void RequireValid(PayRunBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunBaseline> items)
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

/// <summary>Functions of pay run baseline rules.</summary>
public static class PayRunBaselineRules
{
    public static PayRunBaselineValidator DefaultPayRunBaselineValidator()
    {
        return new PayRunBaselineValidator(
            new List<IPayRunBaselineRule>
            {
                new PayRunBaselineArchivedGuardRule(16),
                new PayRunBaselineExpiresAtRangeRule(32, 86400),
                new PayRunBaselineJobCodeLengthRule(1000),
                new PayRunBaselineCodeLengthRule(32),
                new PayRunBaselineAllowedCodeRule(new List<string> { "default", "manual", "seasonal" }),
                new PayRunBaselineGraceDaysRangeRule(80, 255),
            });
    }
}
