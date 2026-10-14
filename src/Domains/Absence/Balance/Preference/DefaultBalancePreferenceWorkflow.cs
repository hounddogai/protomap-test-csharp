namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default balance preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultBalancePreferenceWorkflow(IDefaultBalancePreferenceStore Store, DefaultBalancePreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    public static DefaultBalancePreferenceWorkflow NewDefaultBalancePreferenceWorkflow(IDefaultBalancePreferenceStore store, IClock clock)
    {
        return new DefaultBalancePreferenceWorkflow(
            store,
            DefaultBalancePreferenceRules.DefaultDefaultBalancePreferenceValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default balance preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultBalancePreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default balance preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultBalancePreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
