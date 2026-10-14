namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatNoticeWorkflow(ILicenseSeatNoticeStore Store, LicenseSeatNoticeValidator Validator, IClock Clock, List<string> Journal, LicenseSeatNoticeEngine Engine)
{
    public static LicenseSeatNoticeWorkflow NewLicenseSeatNoticeWorkflow(ILicenseSeatNoticeStore store, IClock clock)
    {
        return new LicenseSeatNoticeWorkflow(
            store,
            LicenseSeatNoticeRules.DefaultLicenseSeatNoticeValidator(),
            clock,
            new List<string>(),
            LicenseSeatNoticeEngine.DefaultLicenseSeatNoticeEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat notice {code} not found");
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
