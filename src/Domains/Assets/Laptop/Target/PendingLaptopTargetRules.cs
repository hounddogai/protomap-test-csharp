namespace Acme.Workforce.Assets.Laptop.Target;

/// <summary>A rule a pending laptop target must pass. Check returns its problems, or none.</summary>
public interface IPendingLaptopTargetRule
{
    List<string> Check(PendingLaptopTarget item);
}

public sealed record PendingLaptopTargetRequiredCodeRule(string Message) : IPendingLaptopTargetRule
{
    public List<string> Check(PendingLaptopTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopTargetExpiresAtRangeRule(int Min, int Max) : IPendingLaptopTargetRule
{
    public List<string> Check(PendingLaptopTarget item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopTargetRequiredReviewerRule(string Message) : IPendingLaptopTargetRule
{
    public List<string> Check(PendingLaptopTarget item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopTargetBatchSizeRangeRule(int Min, int Max) : IPendingLaptopTargetRule
{
    public List<string> Check(PendingLaptopTarget item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending laptop target.</summary>
public sealed record PendingLaptopTargetValidator(List<IPendingLaptopTargetRule> Rules)
{
    public List<string> Validate(PendingLaptopTarget item)
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

    public void RequireValid(PendingLaptopTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLaptopTarget> items)
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

/// <summary>Functions of pending laptop target rules.</summary>
public static class PendingLaptopTargetRules
{
    public static PendingLaptopTargetValidator DefaultPendingLaptopTargetValidator()
    {
        return new PendingLaptopTargetValidator(
            new List<IPendingLaptopTargetRule>
            {
                new PendingLaptopTargetRequiredCodeRule("pending laptop target needs an entry"),
                new PendingLaptopTargetExpiresAtRangeRule(16, 1000),
                new PendingLaptopTargetRequiredReviewerRule("pending laptop target needs a value"),
                new PendingLaptopTargetBatchSizeRangeRule(16, 32),
            });
    }
}
