namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat digest must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatDigestRule
{
    List<string> Check(LicenseSeatDigest item);
}

public sealed record LicenseSeatDigestCostRangeRule(double Min, double Max) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatDigestWeightRangeRule(double Min, double Max) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatDigestCodeLengthRule(int MaxLength) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatDigestCodePrefixRule(string Prefix) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatDigestSequenceRangeRule(int Min, int Max) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatDigestAllowedCodeRule(List<string> Allowed) : ILicenseSeatDigestRule
{
    public List<string> Check(LicenseSeatDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat digest.</summary>
public sealed record LicenseSeatDigestValidator(List<ILicenseSeatDigestRule> Rules)
{
    public List<string> Validate(LicenseSeatDigest item)
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

    public void RequireValid(LicenseSeatDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatDigest> items)
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

/// <summary>Functions of license seat digest rules.</summary>
public static class LicenseSeatDigestRules
{
    public static LicenseSeatDigestValidator DefaultLicenseSeatDigestValidator()
    {
        return new LicenseSeatDigestValidator(
            new List<ILicenseSeatDigestRule>
            {
                new LicenseSeatDigestCostRangeRule(4.0, 2566.0),
                new LicenseSeatDigestWeightRangeRule(7.0, 4157.7),
                new LicenseSeatDigestCodeLengthRule(32),
                new LicenseSeatDigestCodePrefixRule("X"),
                new LicenseSeatDigestSequenceRangeRule(120, 86400),
                new LicenseSeatDigestAllowedCodeRule(new List<string> { "default" }),
            });
    }
}
