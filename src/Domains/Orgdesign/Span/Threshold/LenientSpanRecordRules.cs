namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient span record must pass. Check returns its problems, or none.</summary>
public interface ILenientSpanRecordRule
{
    List<string> Check(LenientSpanRecord item);
}

public sealed record LenientSpanRecordRequiredSkillRule(string Message) : ILenientSpanRecordRule
{
    public List<string> Check(LenientSpanRecord item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRecordLabelLengthRule(int MaxLength) : ILenientSpanRecordRule
{
    public List<string> Check(LenientSpanRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRecordAllowedSkillRule(List<string> Allowed) : ILenientSpanRecordRule
{
    public List<string> Check(LenientSpanRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRecordRequiredLabelRule(string Message) : ILenientSpanRecordRule
{
    public List<string> Check(LenientSpanRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanRecordSkillLengthRule(int MaxLength) : ILenientSpanRecordRule
{
    public List<string> Check(LenientSpanRecord item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient span record.</summary>
public sealed record LenientSpanRecordValidator(List<ILenientSpanRecordRule> Rules)
{
    public List<string> Validate(LenientSpanRecord item)
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

    public void RequireValid(LenientSpanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSpanRecord> items)
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

/// <summary>Functions of lenient span record rules.</summary>
public static class LenientSpanRecordRules
{
    public static LenientSpanRecordValidator DefaultLenientSpanRecordValidator()
    {
        return new LenientSpanRecordValidator(
            new List<ILenientSpanRecordRule>
            {
                new LenientSpanRecordRequiredSkillRule("lenient span record needs an entry"),
                new LenientSpanRecordLabelLengthRule(80),
                new LenientSpanRecordAllowedSkillRule(new List<string> { "lenient", "pending", "batched" }),
                new LenientSpanRecordRequiredLabelRule("lenient span record needs a value"),
                new LenientSpanRecordSkillLengthRule(1000),
            });
    }
}
