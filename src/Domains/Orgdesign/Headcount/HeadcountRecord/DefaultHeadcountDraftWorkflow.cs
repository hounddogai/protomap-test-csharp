namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default headcount drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultHeadcountDraftDesk(IDefaultHeadcountDraftStore Store, DefaultHeadcountDraftValidator Validator, IClock Clock, List<string> Journal, DefaultHeadcountDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultHeadcountDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultHeadcountDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default headcount draft {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of default headcount draft workflow.</summary>
public static class DefaultHeadcountDraftWorkflow
{
    public static DefaultHeadcountDraftDesk NewDefaultHeadcountDraftDesk(IDefaultHeadcountDraftStore store, IClock clock)
    {
        return new DefaultHeadcountDraftDesk(
            store,
            DefaultHeadcountDraftRules.DefaultDefaultHeadcountDraftValidator(),
            clock,
            new List<string>(),
            DefaultHeadcountDraftEngine.DefaultDefaultHeadcountDraftEngine());
    }
}
