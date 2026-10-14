namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending asset return record must pass. Check returns its problems, or none.</summary>
public interface IPendingAssetReturnRecordRule
{
    List<string> Check(PendingAssetReturnRecord item);
}

public sealed record PendingAssetReturnRecordActiveGuardRule(double Limit) : IPendingAssetReturnRecordRule
{
    public List<string> Check(PendingAssetReturnRecord item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnRecordAllowedCodeRule(List<string> Allowed) : IPendingAssetReturnRecordRule
{
    public List<string> Check(PendingAssetReturnRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnRecordRequiredTitleRule(string Message) : IPendingAssetReturnRecordRule
{
    public List<string> Check(PendingAssetReturnRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnRecordExternalRefLengthRule(int MaxLength) : IPendingAssetReturnRecordRule
{
    public List<string> Check(PendingAssetReturnRecord item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending asset return record.</summary>
public sealed record PendingAssetReturnRecordValidator(List<IPendingAssetReturnRecordRule> Rules)
{
    public List<string> Validate(PendingAssetReturnRecord item)
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

    public void RequireValid(PendingAssetReturnRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAssetReturnRecord> items)
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

/// <summary>Functions of pending asset return record rules.</summary>
public static class PendingAssetReturnRecordRules
{
    public static PendingAssetReturnRecordValidator DefaultPendingAssetReturnRecordValidator()
    {
        return new PendingAssetReturnRecordValidator(
            new List<IPendingAssetReturnRecordRule>
            {
                new PendingAssetReturnRecordActiveGuardRule(7.3),
                new PendingAssetReturnRecordAllowedCodeRule(new List<string>()),
                new PendingAssetReturnRecordRequiredTitleRule("pending asset return record needs this field"),
                new PendingAssetReturnRecordExternalRefLengthRule(32),
            });
    }
}
