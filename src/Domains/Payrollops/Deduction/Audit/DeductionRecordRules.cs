namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction record must pass. Check returns its problems, or none.</summary>
public interface IDeductionRecordRule
{
    List<string> Check(DeductionRecord item);
}

public sealed record DeductionRecordAllowedCodeRule(List<string> Allowed) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRecordTeamLengthRule(int MaxLength) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRecordScoreRangeRule(double Min, double Max) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
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

public sealed record DeductionRecordCodeLengthRule(int MaxLength) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRecordRequiredLabelRule(string Message) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionRecordTagsLimitRule(int MaxCount, List<string> Banned) : IDeductionRecordRule
{
    public List<string> Check(DeductionRecord item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction record.</summary>
public sealed record DeductionRecordValidator(List<IDeductionRecordRule> Rules)
{
    public List<string> Validate(DeductionRecord item)
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

    public void RequireValid(DeductionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionRecord> items)
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

/// <summary>Functions of deduction record rules.</summary>
public static class DeductionRecordRules
{
    public static DeductionRecordValidator DefaultDeductionRecordValidator()
    {
        return new DeductionRecordValidator(
            new List<IDeductionRecordRule>
            {
                new DeductionRecordAllowedCodeRule(new List<string> { "rolling" }),
                new DeductionRecordTeamLengthRule(120),
                new DeductionRecordScoreRangeRule(2.8, 3510.2),
                new DeductionRecordCodeLengthRule(64),
                new DeductionRecordRequiredLabelRule("deduction record needs a value"),
                new DeductionRecordTagsLimitRule(32, new List<string> { "strict", "default", "global" }),
            });
    }
}
