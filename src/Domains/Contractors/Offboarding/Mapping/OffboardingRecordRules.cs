namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding record must pass. Check returns its problems, or none.</summary>
public interface IOffboardingRecordRule
{
    List<string> Check(OffboardingRecord item);
}

public sealed record OffboardingRecordCodeLengthRule(int MaxLength) : IOffboardingRecordRule
{
    public List<string> Check(OffboardingRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRecordRequiredLabelRule(string Message) : IOffboardingRecordRule
{
    public List<string> Check(OffboardingRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRecordAllowedLabelRule(List<string> Allowed) : IOffboardingRecordRule
{
    public List<string> Check(OffboardingRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding record.</summary>
public sealed record OffboardingRecordValidator(List<IOffboardingRecordRule> Rules)
{
    public List<string> Validate(OffboardingRecord item)
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

    public void RequireValid(OffboardingRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingRecord> items)
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

/// <summary>Functions of offboarding record rules.</summary>
public static class OffboardingRecordRules
{
    public static OffboardingRecordValidator DefaultOffboardingRecordValidator()
    {
        return new OffboardingRecordValidator(
            new List<IOffboardingRecordRule>
            {
                new OffboardingRecordCodeLengthRule(255),
                new OffboardingRecordRequiredLabelRule("offboarding record needs an entry"),
                new OffboardingRecordAllowedLabelRule(new List<string> { "batched" }),
            });
    }
}
