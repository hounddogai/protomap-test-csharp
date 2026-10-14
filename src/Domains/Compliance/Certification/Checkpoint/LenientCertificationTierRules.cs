namespace Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>A rule a lenient certification tier must pass. Check returns its problems, or none.</summary>
public interface ILenientCertificationTierRule
{
    List<string> Check(LenientCertificationTier item);
}

public sealed record LenientCertificationTierRequiredLabelRule(string Message) : ILenientCertificationTierRule
{
    public List<string> Check(LenientCertificationTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationTierScoreRangeRule(double Min, double Max) : ILenientCertificationTierRule
{
    public List<string> Check(LenientCertificationTier item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationTierNameLengthRule(int MaxLength) : ILenientCertificationTierRule
{
    public List<string> Check(LenientCertificationTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationTierRequiredTeamRule(string Message) : ILenientCertificationTierRule
{
    public List<string> Check(LenientCertificationTier item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationTierLabelLengthRule(int MaxLength) : ILenientCertificationTierRule
{
    public List<string> Check(LenientCertificationTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient certification tier.</summary>
public sealed record LenientCertificationTierValidator(List<ILenientCertificationTierRule> Rules)
{
    public List<string> Validate(LenientCertificationTier item)
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

    public void RequireValid(LenientCertificationTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCertificationTier> items)
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

/// <summary>Functions of lenient certification tier rules.</summary>
public static class LenientCertificationTierRules
{
    public static LenientCertificationTierValidator DefaultLenientCertificationTierValidator()
    {
        return new LenientCertificationTierValidator(
            new List<ILenientCertificationTierRule>
            {
                new LenientCertificationTierRequiredLabelRule("lenient certification tier needs this field"),
                new LenientCertificationTierScoreRangeRule(4.0, 2511.7),
                new LenientCertificationTierNameLengthRule(255),
                new LenientCertificationTierRequiredTeamRule("lenient certification tier needs this field"),
                new LenientCertificationTierLabelLengthRule(3600),
            });
    }
}
