namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary adjustment preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryAdjustmentPreferenceDesk(IPrimaryAdjustmentPreferenceStore Store, PrimaryAdjustmentPreferenceValidator Validator, IClock Clock, List<string> Journal, PrimaryAdjustmentPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAdjustmentPreferenceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAdjustmentPreferenceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary adjustment preference {code} not found");
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

/// <summary>Functions of primary adjustment preference workflow.</summary>
public static class PrimaryAdjustmentPreferenceWorkflow
{
    public static PrimaryAdjustmentPreferenceDesk NewPrimaryAdjustmentPreferenceDesk(IPrimaryAdjustmentPreferenceStore store, IClock clock)
    {
        return new PrimaryAdjustmentPreferenceDesk(
            store,
            PrimaryAdjustmentPreferenceRules.DefaultPrimaryAdjustmentPreferenceValidator(),
            clock,
            new List<string>(),
            PrimaryAdjustmentPreferenceEngine.DefaultPrimaryAdjustmentPreferenceEngine());
    }
}
