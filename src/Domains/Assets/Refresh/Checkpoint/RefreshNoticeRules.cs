namespace Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>A rule a refresh notice must pass. Check returns its problems, or none.</summary>
public interface IRefreshNoticeRule
{
    List<string> Check(RefreshNotice item);
}

public sealed record RefreshNoticePhoneLengthRule(int MaxLength) : IRefreshNoticeRule
{
    public List<string> Check(RefreshNotice item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshNoticeNameLengthRule(int MaxLength) : IRefreshNoticeRule
{
    public List<string> Check(RefreshNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshNoticeCodeLengthRule(int MaxLength) : IRefreshNoticeRule
{
    public List<string> Check(RefreshNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh notice.</summary>
public sealed record RefreshNoticeValidator(List<IRefreshNoticeRule> Rules)
{
    public List<string> Validate(RefreshNotice item)
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

    public void RequireValid(RefreshNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshNotice> items)
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

/// <summary>Functions of refresh notice rules.</summary>
public static class RefreshNoticeRules
{
    public static RefreshNoticeValidator DefaultRefreshNoticeValidator()
    {
        return new RefreshNoticeValidator(
            new List<IRefreshNoticeRule>
            {
                new RefreshNoticePhoneLengthRule(3600),
                new RefreshNoticeNameLengthRule(255),
                new RefreshNoticeCodeLengthRule(3600),
            });
    }
}
