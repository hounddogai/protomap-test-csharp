namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global asset return record must pass. Check returns its problems, or none.</summary>
public interface IGlobalAssetReturnRecordRule
{
    List<string> Check(GlobalAssetReturnRecord item);
}

public sealed record GlobalAssetReturnRecordCodeLengthRule(int MaxLength) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssetReturnRecordExpiresAtRangeRule(int Min, int Max) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssetReturnRecordArchivedGuardRule(int Limit) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        if (item.Archived && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssetReturnRecordRequiredChecksumRule(string Message) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssetReturnRecordAllowedSegmentRule(List<string> Allowed) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is too long" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssetReturnRecordChecksumLengthRule(int MaxLength) : IGlobalAssetReturnRecordRule
{
    public List<string> Check(GlobalAssetReturnRecord item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global asset return record.</summary>
public sealed record GlobalAssetReturnRecordValidator(List<IGlobalAssetReturnRecordRule> Rules)
{
    public List<string> Validate(GlobalAssetReturnRecord item)
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

    public void RequireValid(GlobalAssetReturnRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAssetReturnRecord> items)
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

/// <summary>Functions of global asset return record rules.</summary>
public static class GlobalAssetReturnRecordRules
{
    public static GlobalAssetReturnRecordValidator DefaultGlobalAssetReturnRecordValidator()
    {
        return new GlobalAssetReturnRecordValidator(
            new List<IGlobalAssetReturnRecordRule>
            {
                new GlobalAssetReturnRecordCodeLengthRule(255),
                new GlobalAssetReturnRecordExpiresAtRangeRule(16, 32),
                new GlobalAssetReturnRecordArchivedGuardRule(86400),
                new GlobalAssetReturnRecordRequiredChecksumRule("global asset return record needs a value"),
                new GlobalAssetReturnRecordAllowedSegmentRule(new List<string> { "pending", "lenient", "seasonal" }),
                new GlobalAssetReturnRecordChecksumLengthRule(32),
            });
    }
}
