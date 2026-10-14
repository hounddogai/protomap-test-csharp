namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled skill profile proposals through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledSkillProfileProposalManager(IScheduledSkillProfileProposalStore Store, ScheduledSkillProfileProposalValidator Validator, IClock Clock, List<string> Journal, ScheduledSkillProfileProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled skill profile proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledSkillProfileProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled skill profile proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledSkillProfileProposalStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled skill profile proposal {code} not found");
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

/// <summary>Functions of scheduled skill profile proposal workflow.</summary>
public static class ScheduledSkillProfileProposalWorkflow
{
    public static ScheduledSkillProfileProposalManager NewScheduledSkillProfileProposalManager(IScheduledSkillProfileProposalStore store, IClock clock, Registry flags)
    {
        return new ScheduledSkillProfileProposalManager(
            store,
            ScheduledSkillProfileProposalRules.DefaultScheduledSkillProfileProposalValidator(),
            clock,
            new List<string>(),
            ScheduledSkillProfileProposalEngine.DefaultScheduledSkillProfileProposalEngine(flags));
    }
}
