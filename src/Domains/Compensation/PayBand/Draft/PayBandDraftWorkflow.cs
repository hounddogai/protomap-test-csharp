namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandDraftDesk(IPayBandDraftStore Store, PayBandDraftValidator Validator, IClock Clock, List<string> Journal, PayBandDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandDraftStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandDraftStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band draft {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of pay band draft workflow.</summary>
public static class PayBandDraftWorkflow
{
    public static PayBandDraftDesk NewPayBandDraftDesk(IPayBandDraftStore store, IClock clock)
    {
        return new PayBandDraftDesk(
            store,
            PayBandDraftRules.DefaultPayBandDraftValidator(),
            clock,
            new List<string>(),
            PayBandDraftEngine.DefaultPayBandDraftEngine());
    }
}
