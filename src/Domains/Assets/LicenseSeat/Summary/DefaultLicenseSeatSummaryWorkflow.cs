namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default license seat summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultLicenseSeatSummaryDesk(IDefaultLicenseSeatSummaryStore Store, DefaultLicenseSeatSummaryValidator Validator, IClock Clock, List<string> Journal, DefaultLicenseSeatSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultLicenseSeatSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultLicenseSeatSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat summary {code} not found");
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

/// <summary>Functions of default license seat summary workflow.</summary>
public static class DefaultLicenseSeatSummaryWorkflow
{
    public static DefaultLicenseSeatSummaryDesk NewDefaultLicenseSeatSummaryDesk(IDefaultLicenseSeatSummaryStore store, IClock clock)
    {
        return new DefaultLicenseSeatSummaryDesk(
            store,
            DefaultLicenseSeatSummaryRules.DefaultDefaultLicenseSeatSummaryValidator(),
            clock,
            new List<string>(),
            DefaultLicenseSeatSummaryEngine.DefaultDefaultLicenseSeatSummaryEngine());
    }
}
