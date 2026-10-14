namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary reorg claim must pass. Check returns its problems, or none.</summary>
public interface IPrimaryReorgClaimRule
{
    List<string> Check(PrimaryReorgClaim item);
}

public sealed record PrimaryReorgClaimRequiredSourceRule(string Message) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgClaimRequiredCodeRule(string Message) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgClaimLabelLengthRule(int MaxLength) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgClaimWindowRule(int MaxSpan) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgClaimAllowedCodeRule(List<string> Allowed) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgClaimScoreRangeRule(double Min, double Max) : IPrimaryReorgClaimRule
{
    public List<string> Check(PrimaryReorgClaim item)
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

/// <summary>Runs every rule of a primary reorg claim.</summary>
public sealed record PrimaryReorgClaimValidator(List<IPrimaryReorgClaimRule> Rules)
{
    public List<string> Validate(PrimaryReorgClaim item)
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

    public void RequireValid(PrimaryReorgClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryReorgClaim> items)
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

/// <summary>Functions of primary reorg claim rules.</summary>
public static class PrimaryReorgClaimRules
{
    public static PrimaryReorgClaimValidator DefaultPrimaryReorgClaimValidator()
    {
        return new PrimaryReorgClaimValidator(
            new List<IPrimaryReorgClaimRule>
            {
                new PrimaryReorgClaimRequiredSourceRule("primary reorg claim needs a value"),
                new PrimaryReorgClaimRequiredCodeRule("primary reorg claim needs a value"),
                new PrimaryReorgClaimLabelLengthRule(120),
                new PrimaryReorgClaimWindowRule(16),
                new PrimaryReorgClaimAllowedCodeRule(new List<string> { "strict", "scheduled" }),
                new PrimaryReorgClaimScoreRangeRule(0.2, 4232.0),
            });
    }
}
