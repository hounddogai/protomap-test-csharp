namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default nomination rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultNominationRollupWorkflow(IDefaultNominationRollupStore Store, DefaultNominationRollupValidator Validator, IClock Clock, List<string> Journal, DefaultNominationRollupEngine Engine)
{
    public static DefaultNominationRollupWorkflow NewDefaultNominationRollupWorkflow(IDefaultNominationRollupStore store, IClock clock, Registry flags)
    {
        return new DefaultNominationRollupWorkflow(
            store,
            DefaultNominationRollupRules.DefaultDefaultNominationRollupValidator(),
            clock,
            new List<string>(),
            DefaultNominationRollupEngine.DefaultDefaultNominationRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultNominationRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultNominationRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default nomination rollup {code} not found");
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
