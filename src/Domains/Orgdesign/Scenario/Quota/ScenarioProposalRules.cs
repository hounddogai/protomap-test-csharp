namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario proposal must pass. Check returns its problems, or none.</summary>
public interface IScenarioProposalRule
{
    List<string> Check(ScenarioProposal item);
}

public sealed record ScenarioProposalCreatedAtRangeRule(int Min, int Max) : IScenarioProposalRule
{
    public List<string> Check(ScenarioProposal item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioProposalAllowedJobCodeRule(List<string> Allowed) : IScenarioProposalRule
{
    public List<string> Check(ScenarioProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioProposalJobCodeLengthRule(int MaxLength) : IScenarioProposalRule
{
    public List<string> Check(ScenarioProposal item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioProposalCodeLengthRule(int MaxLength) : IScenarioProposalRule
{
    public List<string> Check(ScenarioProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioProposalTitleLengthRule(int MaxLength) : IScenarioProposalRule
{
    public List<string> Check(ScenarioProposal item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario proposal.</summary>
public sealed record ScenarioProposalValidator(List<IScenarioProposalRule> Rules)
{
    public List<string> Validate(ScenarioProposal item)
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

    public void RequireValid(ScenarioProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioProposal> items)
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

/// <summary>Functions of scenario proposal rules.</summary>
public static class ScenarioProposalRules
{
    public static ScenarioProposalValidator DefaultScenarioProposalValidator()
    {
        return new ScenarioProposalValidator(
            new List<IScenarioProposalRule>
            {
                new ScenarioProposalCreatedAtRangeRule(255, 86400),
                new ScenarioProposalAllowedJobCodeRule(new List<string>()),
                new ScenarioProposalJobCodeLengthRule(3600),
                new ScenarioProposalCodeLengthRule(86400),
                new ScenarioProposalTitleLengthRule(32),
            });
    }
}
