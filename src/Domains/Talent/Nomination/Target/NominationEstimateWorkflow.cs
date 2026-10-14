namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves nomination estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationEstimateWorkflow(INominationEstimateStore Store, NominationEstimateValidator Validator, IClock Clock, List<string> Journal, NominationEstimateEngine Engine)
{
    public static NominationEstimateWorkflow NewNominationEstimateWorkflow(INominationEstimateStore store, IClock clock, Registry flags)
    {
        return new NominationEstimateWorkflow(
            store,
            NominationEstimateRules.DefaultNominationEstimateValidator(),
            clock,
            new List<string>(),
            NominationEstimateEngine.DefaultNominationEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationEstimateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationEstimateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination estimate {code} not found");
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
