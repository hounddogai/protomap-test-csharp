namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending skill profile targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingSkillProfileTargetManager(IPendingSkillProfileTargetStore Store, PendingSkillProfileTargetValidator Validator, IClock Clock, List<string> Journal, PendingSkillProfileTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSkillProfileTargetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSkillProfileTargetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending skill profile target {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pending skill profile target workflow.</summary>
public static class PendingSkillProfileTargetWorkflow
{
    public static PendingSkillProfileTargetManager NewPendingSkillProfileTargetManager(IPendingSkillProfileTargetStore store, IClock clock, Registry flags)
    {
        return new PendingSkillProfileTargetManager(
            store,
            PendingSkillProfileTargetRules.DefaultPendingSkillProfileTargetValidator(),
            clock,
            new List<string>(),
            PendingSkillProfileTargetEngine.DefaultPendingSkillProfileTargetEngine(flags));
    }
}
