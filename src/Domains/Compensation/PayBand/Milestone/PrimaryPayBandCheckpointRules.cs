namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary pay band checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPayBandCheckpointRule
{
    List<string> Check(PrimaryPayBandCheckpoint item);
}

public sealed record PrimaryPayBandCheckpointLabelLengthRule(int MaxLength) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandCheckpointPhoneLengthRule(int MaxLength) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandCheckpointLeaveDaysRangeRule(int Min, int Max) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandCheckpointCodeLengthRule(int MaxLength) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandCheckpointAllowedPhoneRule(List<string> Allowed) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandCheckpointRequiredCodeRule(string Message) : IPrimaryPayBandCheckpointRule
{
    public List<string> Check(PrimaryPayBandCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary pay band checkpoint.</summary>
public sealed record PrimaryPayBandCheckpointValidator(List<IPrimaryPayBandCheckpointRule> Rules)
{
    public List<string> Validate(PrimaryPayBandCheckpoint item)
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

    public void RequireValid(PrimaryPayBandCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPayBandCheckpoint> items)
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

/// <summary>Functions of primary pay band checkpoint rules.</summary>
public static class PrimaryPayBandCheckpointRules
{
    public static PrimaryPayBandCheckpointValidator DefaultPrimaryPayBandCheckpointValidator()
    {
        return new PrimaryPayBandCheckpointValidator(
            new List<IPrimaryPayBandCheckpointRule>
            {
                new PrimaryPayBandCheckpointLabelLengthRule(120),
                new PrimaryPayBandCheckpointPhoneLengthRule(1000),
                new PrimaryPayBandCheckpointLeaveDaysRangeRule(32, 80),
                new PrimaryPayBandCheckpointCodeLengthRule(16),
                new PrimaryPayBandCheckpointAllowedPhoneRule(new List<string> { "adaptive", "lenient" }),
                new PrimaryPayBandCheckpointRequiredCodeRule("primary pay band checkpoint needs a value"),
            });
    }
}
