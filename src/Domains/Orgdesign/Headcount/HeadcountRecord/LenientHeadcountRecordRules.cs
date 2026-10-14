namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient headcount record must pass. Check returns its problems, or none.</summary>
public interface ILenientHeadcountRecordRule
{
    List<string> Check(LenientHeadcountRecord item);
}

public sealed record LenientHeadcountRecordLabelLengthRule(int MaxLength) : ILenientHeadcountRecordRule
{
    public List<string> Check(LenientHeadcountRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHeadcountRecordAllowedSourceRule(List<string> Allowed) : ILenientHeadcountRecordRule
{
    public List<string> Check(LenientHeadcountRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LenientHeadcountRecordScoreRangeRule(double Min, double Max) : ILenientHeadcountRecordRule
{
    public List<string> Check(LenientHeadcountRecord item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient headcount record.</summary>
public sealed record LenientHeadcountRecordValidator(List<ILenientHeadcountRecordRule> Rules)
{
    public List<string> Validate(LenientHeadcountRecord item)
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

    public void RequireValid(LenientHeadcountRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientHeadcountRecord> items)
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

/// <summary>Functions of lenient headcount record rules.</summary>
public static class LenientHeadcountRecordRules
{
    public static LenientHeadcountRecordValidator DefaultLenientHeadcountRecordValidator()
    {
        return new LenientHeadcountRecordValidator(
            new List<ILenientHeadcountRecordRule>
            {
                new LenientHeadcountRecordLabelLengthRule(3600),
                new LenientHeadcountRecordAllowedSourceRule(new List<string> { "manual" }),
                new LenientHeadcountRecordScoreRangeRule(2.1, 730.0),
            });
    }
}
