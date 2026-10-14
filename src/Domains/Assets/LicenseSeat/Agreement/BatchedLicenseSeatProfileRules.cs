namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>A rule a batched license seat profile must pass. Check returns its problems, or none.</summary>
public interface IBatchedLicenseSeatProfileRule
{
    List<string> Check(BatchedLicenseSeatProfile item);
}

public sealed record BatchedLicenseSeatProfileFloorValueRangeRule(double Min, double Max) : IBatchedLicenseSeatProfileRule
{
    public List<string> Check(BatchedLicenseSeatProfile item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLicenseSeatProfileBadgeLengthRule(int MaxLength) : IBatchedLicenseSeatProfileRule
{
    public List<string> Check(BatchedLicenseSeatProfile item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLicenseSeatProfileRequiredJobCodeRule(string Message) : IBatchedLicenseSeatProfileRule
{
    public List<string> Check(BatchedLicenseSeatProfile item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLicenseSeatProfileJobCodeLengthRule(int MaxLength) : IBatchedLicenseSeatProfileRule
{
    public List<string> Check(BatchedLicenseSeatProfile item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLicenseSeatProfileCreatedAtRangeRule(int Min, int Max) : IBatchedLicenseSeatProfileRule
{
    public List<string> Check(BatchedLicenseSeatProfile item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched license seat profile.</summary>
public sealed record BatchedLicenseSeatProfileValidator(List<IBatchedLicenseSeatProfileRule> Rules)
{
    public List<string> Validate(BatchedLicenseSeatProfile item)
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

    public void RequireValid(BatchedLicenseSeatProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLicenseSeatProfile> items)
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

/// <summary>Functions of batched license seat profile rules.</summary>
public static class BatchedLicenseSeatProfileRules
{
    public static BatchedLicenseSeatProfileValidator DefaultBatchedLicenseSeatProfileValidator()
    {
        return new BatchedLicenseSeatProfileValidator(
            new List<IBatchedLicenseSeatProfileRule>
            {
                new BatchedLicenseSeatProfileFloorValueRangeRule(2.4, 2456.6),
                new BatchedLicenseSeatProfileBadgeLengthRule(16),
                new BatchedLicenseSeatProfileRequiredJobCodeRule("batched license seat profile needs a value"),
                new BatchedLicenseSeatProfileJobCodeLengthRule(32),
                new BatchedLicenseSeatProfileCreatedAtRangeRule(64, 80),
            });
    }
}
