namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global career path profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalCareerPathProfileDesk(IGlobalCareerPathProfileStore Store, GlobalCareerPathProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global career path profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalCareerPathProfileStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global career path profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalCareerPathProfileStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of global career path profile workflow.</summary>
public static class GlobalCareerPathProfileWorkflow
{
    public static GlobalCareerPathProfileDesk NewGlobalCareerPathProfileDesk(IGlobalCareerPathProfileStore store, IClock clock)
    {
        return new GlobalCareerPathProfileDesk(
            store,
            GlobalCareerPathProfileRules.DefaultGlobalCareerPathProfileValidator(),
            clock,
            new List<string>());
    }
}
