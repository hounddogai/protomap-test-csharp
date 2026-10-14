namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination variance must pass. Check returns its problems, or none.</summary>
public interface INominationVarianceRule
{
    List<string> Check(NominationVariance item);
}

public sealed record NominationVarianceLeaveDaysRangeRule(int Min, int Max) : INominationVarianceRule
{
    public List<string> Check(NominationVariance item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationVarianceExpiresAtRangeRule(int Min, int Max) : INominationVarianceRule
{
    public List<string> Check(NominationVariance item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationVarianceRequiredCodeRule(string Message) : INominationVarianceRule
{
    public List<string> Check(NominationVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationVarianceAllowedGradeRule(List<string> Allowed) : INominationVarianceRule
{
    public List<string> Check(NominationVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination variance.</summary>
public sealed record NominationVarianceValidator(List<INominationVarianceRule> Rules)
{
    public List<string> Validate(NominationVariance item)
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

    public void RequireValid(NominationVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationVariance> items)
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

/// <summary>Functions of nomination variance rules.</summary>
public static class NominationVarianceRules
{
    public static NominationVarianceValidator DefaultNominationVarianceValidator()
    {
        return new NominationVarianceValidator(
            new List<INominationVarianceRule>
            {
                new NominationVarianceLeaveDaysRangeRule(255, 1000),
                new NominationVarianceExpiresAtRangeRule(16, 32),
                new NominationVarianceRequiredCodeRule("nomination variance needs a value"),
                new NominationVarianceAllowedGradeRule(new List<string> { "batched" }),
            });
    }
}
