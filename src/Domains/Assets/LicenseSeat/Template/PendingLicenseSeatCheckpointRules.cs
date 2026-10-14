namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending license seat checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPendingLicenseSeatCheckpointRule
{
    List<string> Check(PendingLicenseSeatCheckpoint item);
}

public sealed record PendingLicenseSeatCheckpointCodePrefixRule(string Prefix) : IPendingLicenseSeatCheckpointRule
{
    public List<string> Check(PendingLicenseSeatCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatCheckpointRequiredCodeRule(string Message) : IPendingLicenseSeatCheckpointRule
{
    public List<string> Check(PendingLicenseSeatCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatCheckpointSummaryLengthRule(int MaxLength) : IPendingLicenseSeatCheckpointRule
{
    public List<string> Check(PendingLicenseSeatCheckpoint item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatCheckpointWindowDaysRangeRule(int Min, int Max) : IPendingLicenseSeatCheckpointRule
{
    public List<string> Check(PendingLicenseSeatCheckpoint item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatCheckpointAllowedNameRule(List<string> Allowed) : IPendingLicenseSeatCheckpointRule
{
    public List<string> Check(PendingLicenseSeatCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending license seat checkpoint.</summary>
public sealed record PendingLicenseSeatCheckpointValidator(List<IPendingLicenseSeatCheckpointRule> Rules)
{
    public List<string> Validate(PendingLicenseSeatCheckpoint item)
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

    public void RequireValid(PendingLicenseSeatCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLicenseSeatCheckpoint> items)
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

/// <summary>Functions of pending license seat checkpoint rules.</summary>
public static class PendingLicenseSeatCheckpointRules
{
    public static PendingLicenseSeatCheckpointValidator DefaultPendingLicenseSeatCheckpointValidator()
    {
        return new PendingLicenseSeatCheckpointValidator(
            new List<IPendingLicenseSeatCheckpointRule>
            {
                new PendingLicenseSeatCheckpointCodePrefixRule("OPS"),
                new PendingLicenseSeatCheckpointRequiredCodeRule("pending license seat checkpoint needs this field"),
                new PendingLicenseSeatCheckpointSummaryLengthRule(64),
                new PendingLicenseSeatCheckpointWindowDaysRangeRule(64, 1000),
                new PendingLicenseSeatCheckpointAllowedNameRule(new List<string>()),
            });
    }
}
