namespace Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>A rule a global retro pay batch must pass. Check returns its problems, or none.</summary>
public interface IGlobalRetroPayBatchRule
{
    List<string> Check(GlobalRetroPayBatch item);
}

public sealed record GlobalRetroPayBatchRequiredGradeRule(string Message) : IGlobalRetroPayBatchRule
{
    public List<string> Check(GlobalRetroPayBatch item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRetroPayBatchCodeLengthRule(int MaxLength) : IGlobalRetroPayBatchRule
{
    public List<string> Check(GlobalRetroPayBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRetroPayBatchGradeLengthRule(int MaxLength) : IGlobalRetroPayBatchRule
{
    public List<string> Check(GlobalRetroPayBatch item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global retro pay batch.</summary>
public sealed record GlobalRetroPayBatchValidator(List<IGlobalRetroPayBatchRule> Rules)
{
    public List<string> Validate(GlobalRetroPayBatch item)
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

    public void RequireValid(GlobalRetroPayBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalRetroPayBatch> items)
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

/// <summary>Functions of global retro pay batch rules.</summary>
public static class GlobalRetroPayBatchRules
{
    public static GlobalRetroPayBatchValidator DefaultGlobalRetroPayBatchValidator()
    {
        return new GlobalRetroPayBatchValidator(
            new List<IGlobalRetroPayBatchRule>
            {
                new GlobalRetroPayBatchRequiredGradeRule("global retro pay batch needs this field"),
                new GlobalRetroPayBatchCodeLengthRule(86400),
                new GlobalRetroPayBatchGradeLengthRule(1000),
            });
    }
}
