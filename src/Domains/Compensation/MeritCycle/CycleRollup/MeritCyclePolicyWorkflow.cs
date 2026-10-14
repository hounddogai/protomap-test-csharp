namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCyclePolicyManager(IMeritCyclePolicyStore Store, MeritCyclePolicyValidator Validator, IClock Clock, List<string> Journal, MeritCyclePolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCyclePolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCyclePolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle policy {code} not found");
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

/// <summary>Functions of merit cycle policy workflow.</summary>
public static class MeritCyclePolicyWorkflow
{
    public static MeritCyclePolicyManager NewMeritCyclePolicyManager(IMeritCyclePolicyStore store, IClock clock)
    {
        return new MeritCyclePolicyManager(
            store,
            MeritCyclePolicyRules.DefaultMeritCyclePolicyValidator(),
            clock,
            new List<string>(),
            MeritCyclePolicyEngine.DefaultMeritCyclePolicyEngine());
    }
}
