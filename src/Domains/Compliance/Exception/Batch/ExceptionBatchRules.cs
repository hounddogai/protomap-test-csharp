namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception batch must pass. Check returns its problems, or none.</summary>
public interface IExceptionBatchRule
{
    List<string> Check(ExceptionBatch item);
}

public sealed record ExceptionBatchMinimumRangeRule(double Min, double Max) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBatchTitleLengthRule(int MaxLength) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBatchScoreRangeRule(double Min, double Max) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBatchCodeLengthRule(int MaxLength) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBatchAllowedSummaryRule(List<string> Allowed) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBatchRequiredTeamRule(string Message) : IExceptionBatchRule
{
    public List<string> Check(ExceptionBatch item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception batch.</summary>
public sealed record ExceptionBatchValidator(List<IExceptionBatchRule> Rules)
{
    public List<string> Validate(ExceptionBatch item)
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

    public void RequireValid(ExceptionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionBatch> items)
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

/// <summary>Functions of exception batch rules.</summary>
public static class ExceptionBatchRules
{
    public static ExceptionBatchValidator DefaultExceptionBatchValidator()
    {
        return new ExceptionBatchValidator(
            new List<IExceptionBatchRule>
            {
                new ExceptionBatchMinimumRangeRule(4.5, 3296.6),
                new ExceptionBatchTitleLengthRule(86400),
                new ExceptionBatchScoreRangeRule(7.9, 1521.6),
                new ExceptionBatchCodeLengthRule(80),
                new ExceptionBatchAllowedSummaryRule(new List<string> { "lenient", "rolling", "scheduled" }),
                new ExceptionBatchRequiredTeamRule("exception batch needs a value"),
            });
    }
}
