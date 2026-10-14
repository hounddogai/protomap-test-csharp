namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount profile must pass. Check returns its problems, or none.</summary>
public interface IHeadcountProfileRule
{
    List<string> Check(HeadcountProfile item);
}

public sealed record HeadcountProfileAllowedCodeRule(List<string> Allowed) : IHeadcountProfileRule
{
    public List<string> Check(HeadcountProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProfileAmountRangeRule(double Min, double Max) : IHeadcountProfileRule
{
    public List<string> Check(HeadcountProfile item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountProfileCodePrefixRule(string Prefix) : IHeadcountProfileRule
{
    public List<string> Check(HeadcountProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProfileRequiredOwnerIdRule(string Message) : IHeadcountProfileRule
{
    public List<string> Check(HeadcountProfile item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProfileOwnerIdLengthRule(int MaxLength) : IHeadcountProfileRule
{
    public List<string> Check(HeadcountProfile item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount profile.</summary>
public sealed record HeadcountProfileValidator(List<IHeadcountProfileRule> Rules)
{
    public List<string> Validate(HeadcountProfile item)
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

    public void RequireValid(HeadcountProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountProfile> items)
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

/// <summary>Functions of headcount profile rules.</summary>
public static class HeadcountProfileRules
{
    public static HeadcountProfileValidator DefaultHeadcountProfileValidator()
    {
        return new HeadcountProfileValidator(
            new List<IHeadcountProfileRule>
            {
                new HeadcountProfileAllowedCodeRule(new List<string> { "rolling", "lenient", "lenient" }),
                new HeadcountProfileAmountRangeRule(8.6, 2563.8),
                new HeadcountProfileCodePrefixRule("OPS"),
                new HeadcountProfileRequiredOwnerIdRule("headcount profile needs this field"),
                new HeadcountProfileOwnerIdLengthRule(86400),
            });
    }
}
