namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped license seat checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICappedLicenseSeatCheckpointRule
{
    List<string> Check(CappedLicenseSeatCheckpoint item);
}

public sealed record CappedLicenseSeatCheckpointBatchSizeRangeRule(int Min, int Max) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
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

public sealed record CappedLicenseSeatCheckpointCodePrefixRule(string Prefix) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCheckpointRequiredCostCenterRule(string Message) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCheckpointAllowedCostCenterRule(List<string> Allowed) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCheckpointTitleLengthRule(int MaxLength) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCheckpointCodeLengthRule(int MaxLength) : ICappedLicenseSeatCheckpointRule
{
    public List<string> Check(CappedLicenseSeatCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped license seat checkpoint.</summary>
public sealed record CappedLicenseSeatCheckpointValidator(List<ICappedLicenseSeatCheckpointRule> Rules)
{
    public List<string> Validate(CappedLicenseSeatCheckpoint item)
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

    public void RequireValid(CappedLicenseSeatCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLicenseSeatCheckpoint> items)
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

/// <summary>Functions of capped license seat checkpoint rules.</summary>
public static class CappedLicenseSeatCheckpointRules
{
    public static CappedLicenseSeatCheckpointValidator DefaultCappedLicenseSeatCheckpointValidator()
    {
        return new CappedLicenseSeatCheckpointValidator(
            new List<ICappedLicenseSeatCheckpointRule>
            {
                new CappedLicenseSeatCheckpointBatchSizeRangeRule(16, 255),
                new CappedLicenseSeatCheckpointCodePrefixRule("REF"),
                new CappedLicenseSeatCheckpointRequiredCostCenterRule(
                    "capped license seat checkpoint needs this field"),
                new CappedLicenseSeatCheckpointAllowedCostCenterRule(new List<string>()),
                new CappedLicenseSeatCheckpointTitleLengthRule(16),
                new CappedLicenseSeatCheckpointCodeLengthRule(80),
            });
    }
}
