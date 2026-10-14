namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary succession plan rollup must pass. Check returns its problems, or none.</summary>
public interface IPrimarySuccessionPlanRollupRule
{
    List<string> Check(PrimarySuccessionPlanRollup item);
}

public sealed record PrimarySuccessionPlanRollupActiveGuardRule(int Limit) : IPrimarySuccessionPlanRollupRule
{
    public List<string> Check(PrimarySuccessionPlanRollup item)
    {
        if (item.Active && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRollupAllowedLabelRule(List<string> Allowed) : IPrimarySuccessionPlanRollupRule
{
    public List<string> Check(PrimarySuccessionPlanRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRollupChannelLengthRule(int MaxLength) : IPrimarySuccessionPlanRollupRule
{
    public List<string> Check(PrimarySuccessionPlanRollup item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRollupRequiredCodeRule(string Message) : IPrimarySuccessionPlanRollupRule
{
    public List<string> Check(PrimarySuccessionPlanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRollupLabelLengthRule(int MaxLength) : IPrimarySuccessionPlanRollupRule
{
    public List<string> Check(PrimarySuccessionPlanRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary succession plan rollup.</summary>
public sealed record PrimarySuccessionPlanRollupValidator(List<IPrimarySuccessionPlanRollupRule> Rules)
{
    public List<string> Validate(PrimarySuccessionPlanRollup item)
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

    public void RequireValid(PrimarySuccessionPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySuccessionPlanRollup> items)
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

/// <summary>Functions of primary succession plan rollup rules.</summary>
public static class PrimarySuccessionPlanRollupRules
{
    public static PrimarySuccessionPlanRollupValidator DefaultPrimarySuccessionPlanRollupValidator()
    {
        return new PrimarySuccessionPlanRollupValidator(
            new List<IPrimarySuccessionPlanRollupRule>
            {
                new PrimarySuccessionPlanRollupActiveGuardRule(255),
                new PrimarySuccessionPlanRollupAllowedLabelRule(new List<string>()),
                new PrimarySuccessionPlanRollupChannelLengthRule(64),
                new PrimarySuccessionPlanRollupRequiredCodeRule("primary succession plan rollup needs an entry"),
                new PrimarySuccessionPlanRollupLabelLengthRule(1000),
            });
    }
}
