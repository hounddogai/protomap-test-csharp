namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary retro pay preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryRetroPayPreferenceWorkflow(IPrimaryRetroPayPreferenceStore Store, PrimaryRetroPayPreferenceValidator Validator, IClock Clock, List<string> Journal, PrimaryRetroPayPreferenceEngine Engine)
{
    public static PrimaryRetroPayPreferenceWorkflow NewPrimaryRetroPayPreferenceWorkflow(IPrimaryRetroPayPreferenceStore store, IClock clock, Registry flags)
    {
        return new PrimaryRetroPayPreferenceWorkflow(
            store,
            PrimaryRetroPayPreferenceRules.DefaultPrimaryRetroPayPreferenceValidator(),
            clock,
            new List<string>(),
            PrimaryRetroPayPreferenceEngine.DefaultPrimaryRetroPayPreferenceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary retro pay preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryRetroPayPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary retro pay preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryRetroPayPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary retro pay preference {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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
