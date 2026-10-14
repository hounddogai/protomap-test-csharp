namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default career path exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultCareerPathExceptionService(IDefaultCareerPathExceptionStore Store, DefaultCareerPathExceptionValidator Validator, IClock Clock, List<string> Journal, DefaultCareerPathExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default career path exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultCareerPathExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default career path exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultCareerPathExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default career path exception {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.StartsAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of default career path exception workflow.</summary>
public static class DefaultCareerPathExceptionWorkflow
{
    public static DefaultCareerPathExceptionService NewDefaultCareerPathExceptionService(IDefaultCareerPathExceptionStore store, IClock clock, Registry flags)
    {
        return new DefaultCareerPathExceptionService(
            store,
            DefaultCareerPathExceptionRules.DefaultDefaultCareerPathExceptionValidator(),
            clock,
            new List<string>(),
            DefaultCareerPathExceptionEngine.DefaultDefaultCareerPathExceptionEngine(flags));
    }
}
