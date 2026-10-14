namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped succession plan cycle must pass. Check returns its problems, or none.</summary>
public interface ICappedSuccessionPlanCycleRule
{
    List<string> Check(CappedSuccessionPlanCycle item);
}

public sealed record CappedSuccessionPlanCycleAllowedGradeRule(List<string> Allowed) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record CappedSuccessionPlanCycleRequiredGradeRule(string Message) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSuccessionPlanCycleRequiredLabelRule(string Message) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSuccessionPlanCycleCodeLengthRule(int MaxLength) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSuccessionPlanCycleLabelLengthRule(int MaxLength) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSuccessionPlanCycleChannelLengthRule(int MaxLength) : ICappedSuccessionPlanCycleRule
{
    public List<string> Check(CappedSuccessionPlanCycle item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped succession plan cycle.</summary>
public sealed record CappedSuccessionPlanCycleValidator(List<ICappedSuccessionPlanCycleRule> Rules)
{
    public List<string> Validate(CappedSuccessionPlanCycle item)
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

    public void RequireValid(CappedSuccessionPlanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSuccessionPlanCycle> items)
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

/// <summary>Functions of capped succession plan cycle rules.</summary>
public static class CappedSuccessionPlanCycleRules
{
    public static CappedSuccessionPlanCycleValidator DefaultCappedSuccessionPlanCycleValidator()
    {
        return new CappedSuccessionPlanCycleValidator(
            new List<ICappedSuccessionPlanCycleRule>
            {
                new CappedSuccessionPlanCycleAllowedGradeRule(new List<string> { "scheduled" }),
                new CappedSuccessionPlanCycleRequiredGradeRule("capped succession plan cycle needs a value"),
                new CappedSuccessionPlanCycleRequiredLabelRule("capped succession plan cycle needs a value"),
                new CappedSuccessionPlanCycleCodeLengthRule(86400),
                new CappedSuccessionPlanCycleLabelLengthRule(1000),
                new CappedSuccessionPlanCycleChannelLengthRule(1000),
            });
    }
}
