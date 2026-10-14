namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling nomination mapping must pass. Check returns its problems, or none.</summary>
public interface IRollingNominationMappingRule
{
    List<string> Check(RollingNominationMapping item);
}

public sealed record RollingNominationMappingLabelLengthRule(int MaxLength) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationMappingRequiredCodeRule(string Message) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationMappingCodeLengthRule(int MaxLength) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationMappingRequiredLabelRule(string Message) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationMappingSequenceRangeRule(int Min, int Max) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationMappingAllowedCodeRule(List<string> Allowed) : IRollingNominationMappingRule
{
    public List<string> Check(RollingNominationMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling nomination mapping.</summary>
public sealed record RollingNominationMappingValidator(List<IRollingNominationMappingRule> Rules)
{
    public List<string> Validate(RollingNominationMapping item)
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

    public void RequireValid(RollingNominationMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingNominationMapping> items)
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

/// <summary>Functions of rolling nomination mapping rules.</summary>
public static class RollingNominationMappingRules
{
    public static RollingNominationMappingValidator DefaultRollingNominationMappingValidator()
    {
        return new RollingNominationMappingValidator(
            new List<IRollingNominationMappingRule>
            {
                new RollingNominationMappingLabelLengthRule(255),
                new RollingNominationMappingRequiredCodeRule("rolling nomination mapping needs a value"),
                new RollingNominationMappingCodeLengthRule(64),
                new RollingNominationMappingRequiredLabelRule("rolling nomination mapping needs a value"),
                new RollingNominationMappingSequenceRangeRule(80, 3600),
                new RollingNominationMappingAllowedCodeRule(new List<string>()),
            });
    }
}
