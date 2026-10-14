namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationPolicyManager(INominationPolicyStore Store, NominationPolicyValidator Validator, IClock Clock, List<string> Journal, NominationPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationPolicyStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationPolicyStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination policy {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of nomination policy workflow.</summary>
public static class NominationPolicyWorkflow
{
    public static NominationPolicyManager NewNominationPolicyManager(INominationPolicyStore store, IClock clock)
    {
        return new NominationPolicyManager(
            store,
            NominationPolicyRules.DefaultNominationPolicyValidator(),
            clock,
            new List<string>(),
            NominationPolicyEngine.DefaultNominationPolicyEngine());
    }
}
