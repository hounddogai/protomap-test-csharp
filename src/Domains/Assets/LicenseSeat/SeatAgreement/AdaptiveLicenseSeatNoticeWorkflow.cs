namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive license seat notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveLicenseSeatNoticeManager(IAdaptiveLicenseSeatNoticeStore Store, AdaptiveLicenseSeatNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive license seat notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveLicenseSeatNoticeStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive license seat notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveLicenseSeatNoticeStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of adaptive license seat notice workflow.</summary>
public static class AdaptiveLicenseSeatNoticeWorkflow
{
    public static AdaptiveLicenseSeatNoticeManager NewAdaptiveLicenseSeatNoticeManager(IAdaptiveLicenseSeatNoticeStore store, IClock clock)
    {
        return new AdaptiveLicenseSeatNoticeManager(
            store,
            AdaptiveLicenseSeatNoticeRules.DefaultAdaptiveLicenseSeatNoticeValidator(),
            clock,
            new List<string>());
    }
}
