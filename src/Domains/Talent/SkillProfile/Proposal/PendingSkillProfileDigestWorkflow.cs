namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending skill profile digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingSkillProfileDigestDesk(IPendingSkillProfileDigestStore Store, PendingSkillProfileDigestValidator Validator, IClock Clock, List<string> Journal, PendingSkillProfileDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSkillProfileDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSkillProfileDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile digest {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of pending skill profile digest workflow.</summary>
public static class PendingSkillProfileDigestWorkflow
{
    public static PendingSkillProfileDigestDesk NewPendingSkillProfileDigestDesk(IPendingSkillProfileDigestStore store, IClock clock, Registry flags)
    {
        return new PendingSkillProfileDigestDesk(
            store,
            PendingSkillProfileDigestRules.DefaultPendingSkillProfileDigestValidator(),
            clock,
            new List<string>(),
            PendingSkillProfileDigestEngine.DefaultPendingSkillProfileDigestEngine(flags));
    }
}
