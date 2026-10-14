namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped training due target must pass. Check returns its problems, or none.</summary>
public interface ICappedTrainingDueTargetRule
{
    List<string> Check(CappedTrainingDueTarget item);
}

public sealed record CappedTrainingDueTargetRequiredChecksumRule(string Message) : ICappedTrainingDueTargetRule
{
    public List<string> Check(CappedTrainingDueTarget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueTargetAllowedCodeRule(List<string> Allowed) : ICappedTrainingDueTargetRule
{
    public List<string> Check(CappedTrainingDueTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueTargetChecksumLengthRule(int MaxLength) : ICappedTrainingDueTargetRule
{
    public List<string> Check(CappedTrainingDueTarget item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueTargetLabelLengthRule(int MaxLength) : ICappedTrainingDueTargetRule
{
    public List<string> Check(CappedTrainingDueTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueTargetRequiredLabelRule(string Message) : ICappedTrainingDueTargetRule
{
    public List<string> Check(CappedTrainingDueTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped training due target.</summary>
public sealed record CappedTrainingDueTargetValidator(List<ICappedTrainingDueTargetRule> Rules)
{
    public List<string> Validate(CappedTrainingDueTarget item)
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

    public void RequireValid(CappedTrainingDueTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTrainingDueTarget> items)
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

/// <summary>Functions of capped training due target rules.</summary>
public static class CappedTrainingDueTargetRules
{
    public static CappedTrainingDueTargetValidator DefaultCappedTrainingDueTargetValidator()
    {
        return new CappedTrainingDueTargetValidator(
            new List<ICappedTrainingDueTargetRule>
            {
                new CappedTrainingDueTargetRequiredChecksumRule("capped training due target needs an entry"),
                new CappedTrainingDueTargetAllowedCodeRule(new List<string> { "capped", "legacy", "tiered" }),
                new CappedTrainingDueTargetChecksumLengthRule(86400),
                new CappedTrainingDueTargetLabelLengthRule(3600),
                new CappedTrainingDueTargetRequiredLabelRule("capped training due target needs this field"),
            });
    }
}
