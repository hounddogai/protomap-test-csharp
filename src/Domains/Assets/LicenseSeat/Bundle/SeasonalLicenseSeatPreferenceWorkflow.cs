namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal license seat preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalLicenseSeatPreferenceWorkflow(ISeasonalLicenseSeatPreferenceStore Store, SeasonalLicenseSeatPreferenceValidator Validator, IClock Clock, List<string> Journal, SeasonalLicenseSeatPreferenceEngine Engine)
{
    public static SeasonalLicenseSeatPreferenceWorkflow NewSeasonalLicenseSeatPreferenceWorkflow(ISeasonalLicenseSeatPreferenceStore store, IClock clock, Registry flags)
    {
        return new SeasonalLicenseSeatPreferenceWorkflow(
            store,
            SeasonalLicenseSeatPreferenceRules.DefaultSeasonalLicenseSeatPreferenceValidator(),
            clock,
            new List<string>(),
            SeasonalLicenseSeatPreferenceEngine.DefaultSeasonalLicenseSeatPreferenceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalLicenseSeatPreferenceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalLicenseSeatPreferenceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal license seat preference {code} not found");
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
