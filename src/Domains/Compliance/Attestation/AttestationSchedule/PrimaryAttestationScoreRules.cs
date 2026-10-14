namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>A rule a primary attestation score must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAttestationScoreRule
{
    List<string> Check(PrimaryAttestationScore item);
}

public sealed record PrimaryAttestationScoreRequiredCategoryRule(string Message) : IPrimaryAttestationScoreRule
{
    public List<string> Check(PrimaryAttestationScore item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAttestationScoreVersionRangeRule(int Min, int Max) : IPrimaryAttestationScoreRule
{
    public List<string> Check(PrimaryAttestationScore item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAttestationScoreTeamLengthRule(int MaxLength) : IPrimaryAttestationScoreRule
{
    public List<string> Check(PrimaryAttestationScore item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAttestationScoreWeightRangeRule(double Min, double Max) : IPrimaryAttestationScoreRule
{
    public List<string> Check(PrimaryAttestationScore item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAttestationScoreRequiredNameRule(string Message) : IPrimaryAttestationScoreRule
{
    public List<string> Check(PrimaryAttestationScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary attestation score.</summary>
public sealed record PrimaryAttestationScoreValidator(List<IPrimaryAttestationScoreRule> Rules)
{
    public List<string> Validate(PrimaryAttestationScore item)
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

    public void RequireValid(PrimaryAttestationScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAttestationScore> items)
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

/// <summary>Functions of primary attestation score rules.</summary>
public static class PrimaryAttestationScoreRules
{
    public static PrimaryAttestationScoreValidator DefaultPrimaryAttestationScoreValidator()
    {
        return new PrimaryAttestationScoreValidator(
            new List<IPrimaryAttestationScoreRule>
            {
                new PrimaryAttestationScoreRequiredCategoryRule("primary attestation score needs this field"),
                new PrimaryAttestationScoreVersionRangeRule(32, 32),
                new PrimaryAttestationScoreTeamLengthRule(3600),
                new PrimaryAttestationScoreWeightRangeRule(9.2, 4218.8),
                new PrimaryAttestationScoreRequiredNameRule("primary attestation score needs a value"),
            });
    }
}
