namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAccrualCheckpointRule
{
    List<string> Check(AccrualCheckpoint item);
}

public sealed record AccrualCheckpointTitleLengthRule(int MaxLength) : IAccrualCheckpointRule
{
    public List<string> Check(AccrualCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCheckpointRequiredTitleRule(string Message) : IAccrualCheckpointRule
{
    public List<string> Check(AccrualCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualCheckpointDescriptionLengthRule(int MaxLength) : IAccrualCheckpointRule
{
    public List<string> Check(AccrualCheckpoint item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCheckpointCodeLengthRule(int MaxLength) : IAccrualCheckpointRule
{
    public List<string> Check(AccrualCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCheckpointAllowedTitleRule(List<string> Allowed) : IAccrualCheckpointRule
{
    public List<string> Check(AccrualCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual checkpoint.</summary>
public sealed record AccrualCheckpointValidator(List<IAccrualCheckpointRule> Rules)
{
    public List<string> Validate(AccrualCheckpoint item)
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

    public void RequireValid(AccrualCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualCheckpoint> items)
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

/// <summary>Functions of accrual checkpoint rules.</summary>
public static class AccrualCheckpointRules
{
    public static AccrualCheckpointValidator DefaultAccrualCheckpointValidator()
    {
        return new AccrualCheckpointValidator(
            new List<IAccrualCheckpointRule>
            {
                new AccrualCheckpointTitleLengthRule(32),
                new AccrualCheckpointRequiredTitleRule("accrual checkpoint needs this field"),
                new AccrualCheckpointDescriptionLengthRule(16),
                new AccrualCheckpointCodeLengthRule(3600),
                new AccrualCheckpointAllowedTitleRule(new List<string> { "fallback" }),
            });
    }
}
