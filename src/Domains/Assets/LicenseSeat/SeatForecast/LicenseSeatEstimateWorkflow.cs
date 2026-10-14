namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatEstimateManager(ILicenseSeatEstimateStore Store, LicenseSeatEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatEstimateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatEstimateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of license seat estimate workflow.</summary>
public static class LicenseSeatEstimateWorkflow
{
    public static LicenseSeatEstimateManager NewLicenseSeatEstimateManager(ILicenseSeatEstimateStore store, IClock clock)
    {
        return new LicenseSeatEstimateManager(
            store,
            LicenseSeatEstimateRules.DefaultLicenseSeatEstimateValidator(),
            clock,
            new List<string>());
    }
}
