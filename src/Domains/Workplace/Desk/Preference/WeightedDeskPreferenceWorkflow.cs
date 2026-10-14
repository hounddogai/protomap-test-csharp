namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted desk preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedDeskPreferenceWorkflow(IWeightedDeskPreferenceStore Store, WeightedDeskPreferenceValidator Validator, IClock Clock, List<string> Journal, WeightedDeskPreferenceEngine Engine)
{
    public static WeightedDeskPreferenceWorkflow NewWeightedDeskPreferenceWorkflow(IWeightedDeskPreferenceStore store, IClock clock)
    {
        return new WeightedDeskPreferenceWorkflow(
            store,
            WeightedDeskPreferenceRules.DefaultWeightedDeskPreferenceValidator(),
            clock,
            new List<string>(),
            WeightedDeskPreferenceEngine.DefaultWeightedDeskPreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedDeskPreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedDeskPreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk preference {code} not found");
        }
        var result = Engine.Compute(item);
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
