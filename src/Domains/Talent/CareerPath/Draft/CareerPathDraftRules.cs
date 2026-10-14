namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>A rule a career path draft must pass. Check returns its problems, or none.</summary>
public interface ICareerPathDraftRule
{
    List<string> Check(CareerPathDraft item);
}

public sealed record CareerPathDraftRequiredContactEmailRule(string Message) : ICareerPathDraftRule
{
    public List<string> Check(CareerPathDraft item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathDraftRequiredLabelRule(string Message) : ICareerPathDraftRule
{
    public List<string> Check(CareerPathDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathDraftRequiredSegmentRule(string Message) : ICareerPathDraftRule
{
    public List<string> Check(CareerPathDraft item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path draft.</summary>
public sealed record CareerPathDraftValidator(List<ICareerPathDraftRule> Rules)
{
    public List<string> Validate(CareerPathDraft item)
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

    public void RequireValid(CareerPathDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathDraft> items)
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

/// <summary>Functions of career path draft rules.</summary>
public static class CareerPathDraftRules
{
    public static CareerPathDraftValidator DefaultCareerPathDraftValidator()
    {
        return new CareerPathDraftValidator(
            new List<ICareerPathDraftRule>
            {
                new CareerPathDraftRequiredContactEmailRule("career path draft needs an entry"),
                new CareerPathDraftRequiredLabelRule("career path draft needs a value"),
                new CareerPathDraftRequiredSegmentRule("career path draft needs an entry"),
            });
    }
}
