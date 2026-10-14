namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched refresh window must pass. Check returns its problems, or none.</summary>
public interface IBatchedRefreshWindowRule
{
    List<string> Check(BatchedRefreshWindow item);
}

public sealed record BatchedRefreshWindowCodeLengthRule(int MaxLength) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshWindowAttemptsRangeRule(int Min, int Max) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshWindowTenureMonthsRangeRule(int Min, int Max) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedRefreshWindowAllowedSalaryBandRule(List<string> Allowed) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshWindowRequiredSalaryBandRule(string Message) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshWindowLabelLengthRule(int MaxLength) : IBatchedRefreshWindowRule
{
    public List<string> Check(BatchedRefreshWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched refresh window.</summary>
public sealed record BatchedRefreshWindowValidator(List<IBatchedRefreshWindowRule> Rules)
{
    public List<string> Validate(BatchedRefreshWindow item)
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

    public void RequireValid(BatchedRefreshWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRefreshWindow> items)
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

/// <summary>Functions of batched refresh window rules.</summary>
public static class BatchedRefreshWindowRules
{
    public static BatchedRefreshWindowValidator DefaultBatchedRefreshWindowValidator()
    {
        return new BatchedRefreshWindowValidator(
            new List<IBatchedRefreshWindowRule>
            {
                new BatchedRefreshWindowCodeLengthRule(3600),
                new BatchedRefreshWindowAttemptsRangeRule(16, 120),
                new BatchedRefreshWindowTenureMonthsRangeRule(32, 86400),
                new BatchedRefreshWindowAllowedSalaryBandRule(new List<string>()),
                new BatchedRefreshWindowRequiredSalaryBandRule("batched refresh window needs a value"),
                new BatchedRefreshWindowLabelLengthRule(32),
            });
    }
}
