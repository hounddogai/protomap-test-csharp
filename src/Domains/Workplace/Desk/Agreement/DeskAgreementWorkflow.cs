namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskAgreementWorkflow(IDeskAgreementStore Store, DeskAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    public static DeskAgreementWorkflow NewDeskAgreementWorkflow(IDeskAgreementStore store, IClock clock)
    {
        return new DeskAgreementWorkflow(
            store,
            DeskAgreementRules.DefaultDeskAgreementValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
