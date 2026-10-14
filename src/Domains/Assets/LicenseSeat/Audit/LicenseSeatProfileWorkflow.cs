namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatProfileCoordinator(ILicenseSeatProfileStore Store, LicenseSeatProfileValidator Validator, IClock Clock, List<string> Journal, LicenseSeatProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of license seat profile workflow.</summary>
public static class LicenseSeatProfileWorkflow
{
    public static LicenseSeatProfileCoordinator NewLicenseSeatProfileCoordinator(ILicenseSeatProfileStore store, IClock clock)
    {
        return new LicenseSeatProfileCoordinator(
            store,
            LicenseSeatProfileRules.DefaultLicenseSeatProfileValidator(),
            clock,
            new List<string>(),
            LicenseSeatProfileEngine.DefaultLicenseSeatProfileEngine());
    }
}
