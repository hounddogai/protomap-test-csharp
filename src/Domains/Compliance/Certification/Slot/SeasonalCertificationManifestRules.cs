namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal certification manifest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCertificationManifestRule
{
    List<string> Check(SeasonalCertificationManifest item);
}

public sealed record SeasonalCertificationManifestAllowedLabelRule(List<string> Allowed) : ISeasonalCertificationManifestRule
{
    public List<string> Check(SeasonalCertificationManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationManifestRequiredTeamRule(string Message) : ISeasonalCertificationManifestRule
{
    public List<string> Check(SeasonalCertificationManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationManifestTeamLengthRule(int MaxLength) : ISeasonalCertificationManifestRule
{
    public List<string> Check(SeasonalCertificationManifest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationManifestRequiredLabelRule(string Message) : ISeasonalCertificationManifestRule
{
    public List<string> Check(SeasonalCertificationManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal certification manifest.</summary>
public sealed record SeasonalCertificationManifestValidator(List<ISeasonalCertificationManifestRule> Rules)
{
    public List<string> Validate(SeasonalCertificationManifest item)
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

    public void RequireValid(SeasonalCertificationManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCertificationManifest> items)
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

/// <summary>Functions of seasonal certification manifest rules.</summary>
public static class SeasonalCertificationManifestRules
{
    public static SeasonalCertificationManifestValidator DefaultSeasonalCertificationManifestValidator()
    {
        return new SeasonalCertificationManifestValidator(
            new List<ISeasonalCertificationManifestRule>
            {
                new SeasonalCertificationManifestAllowedLabelRule(new List<string>()),
                new SeasonalCertificationManifestRequiredTeamRule("seasonal certification manifest needs a value"),
                new SeasonalCertificationManifestTeamLengthRule(255),
                new SeasonalCertificationManifestRequiredLabelRule("seasonal certification manifest needs an entry"),
            });
    }
}
