namespace Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>A rule a capped nomination assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedNominationAssignmentRule
{
    List<string> Check(CappedNominationAssignment item);
}

public sealed record CappedNominationAssignmentCodeLengthRule(int MaxLength) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationAssignmentCountryLengthRule(int MaxLength) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationAssignmentCapacityRangeRule(int Min, int Max) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedNominationAssignmentCodePrefixRule(string Prefix) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationAssignmentGraceDaysRangeRule(int Min, int Max) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationAssignmentExternalRefLengthRule(int MaxLength) : ICappedNominationAssignmentRule
{
    public List<string> Check(CappedNominationAssignment item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped nomination assignment.</summary>
public sealed record CappedNominationAssignmentValidator(List<ICappedNominationAssignmentRule> Rules)
{
    public List<string> Validate(CappedNominationAssignment item)
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

    public void RequireValid(CappedNominationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedNominationAssignment> items)
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

/// <summary>Functions of capped nomination assignment rules.</summary>
public static class CappedNominationAssignmentRules
{
    public static CappedNominationAssignmentValidator DefaultCappedNominationAssignmentValidator()
    {
        return new CappedNominationAssignmentValidator(
            new List<ICappedNominationAssignmentRule>
            {
                new CappedNominationAssignmentCodeLengthRule(86400),
                new CappedNominationAssignmentCountryLengthRule(255),
                new CappedNominationAssignmentCapacityRangeRule(32, 255),
                new CappedNominationAssignmentCodePrefixRule("X"),
                new CappedNominationAssignmentGraceDaysRangeRule(64, 80),
                new CappedNominationAssignmentExternalRefLengthRule(255),
            });
    }
}
