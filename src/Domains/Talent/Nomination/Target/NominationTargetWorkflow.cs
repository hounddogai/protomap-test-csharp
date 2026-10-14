namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationTargetService(INominationTargetStore Store, NominationTargetValidator Validator, IClock Clock, List<string> Journal, NominationTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination target {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of nomination target workflow.</summary>
public static class NominationTargetWorkflow
{
    public static NominationTargetService NewNominationTargetService(INominationTargetStore store, IClock clock)
    {
        return new NominationTargetService(
            store,
            NominationTargetRules.DefaultNominationTargetValidator(),
            clock,
            new List<string>(),
            NominationTargetEngine.DefaultNominationTargetEngine());
    }
}
