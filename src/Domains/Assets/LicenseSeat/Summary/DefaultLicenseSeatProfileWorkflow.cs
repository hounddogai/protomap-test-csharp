namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default license seat profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultLicenseSeatProfileCoordinator(IDefaultLicenseSeatProfileStore Store, DefaultLicenseSeatProfileValidator Validator, IClock Clock, List<string> Journal, DefaultLicenseSeatProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultLicenseSeatProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultLicenseSeatProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default license seat profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of default license seat profile workflow.</summary>
public static class DefaultLicenseSeatProfileWorkflow
{
    public static DefaultLicenseSeatProfileCoordinator NewDefaultLicenseSeatProfileCoordinator(IDefaultLicenseSeatProfileStore store, IClock clock)
    {
        return new DefaultLicenseSeatProfileCoordinator(
            store,
            DefaultLicenseSeatProfileRules.DefaultDefaultLicenseSeatProfileValidator(),
            clock,
            new List<string>(),
            DefaultLicenseSeatProfileEngine.DefaultDefaultLicenseSeatProfileEngine());
    }
}
