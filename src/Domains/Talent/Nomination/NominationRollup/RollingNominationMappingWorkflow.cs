namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling nomination mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingNominationMappingWorkflow(IRollingNominationMappingStore Store, RollingNominationMappingValidator Validator, IClock Clock, List<string> Journal, RollingNominationMappingEngine Engine)
{
    public static RollingNominationMappingWorkflow NewRollingNominationMappingWorkflow(IRollingNominationMappingStore store, IClock clock, Registry flags)
    {
        return new RollingNominationMappingWorkflow(
            store,
            RollingNominationMappingRules.DefaultRollingNominationMappingValidator(),
            clock,
            new List<string>(),
            RollingNominationMappingEngine.DefaultRollingNominationMappingEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling nomination mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingNominationMappingStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling nomination mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingNominationMappingStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling nomination mapping {code} not found");
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
