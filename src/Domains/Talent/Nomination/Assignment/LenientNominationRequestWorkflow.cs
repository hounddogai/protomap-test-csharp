namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient nomination requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientNominationRequestWorkflow(ILenientNominationRequestStore Store, LenientNominationRequestValidator Validator, IClock Clock, List<string> Journal, LenientNominationRequestEngine Engine)
{
    public static LenientNominationRequestWorkflow NewLenientNominationRequestWorkflow(ILenientNominationRequestStore store, IClock clock)
    {
        return new LenientNominationRequestWorkflow(
            store,
            LenientNominationRequestRules.DefaultLenientNominationRequestValidator(),
            clock,
            new List<string>(),
            LenientNominationRequestEngine.DefaultLenientNominationRequestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientNominationRequestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientNominationRequestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient nomination request {code} not found");
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
