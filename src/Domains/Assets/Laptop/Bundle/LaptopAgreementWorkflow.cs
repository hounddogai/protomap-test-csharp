namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopAgreementService(ILaptopAgreementStore Store, LaptopAgreementValidator Validator, IClock Clock, List<string> Journal, LaptopAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of laptop agreement workflow.</summary>
public static class LaptopAgreementWorkflow
{
    public static LaptopAgreementService NewLaptopAgreementService(ILaptopAgreementStore store, IClock clock)
    {
        return new LaptopAgreementService(
            store,
            LaptopAgreementRules.DefaultLaptopAgreementValidator(),
            clock,
            new List<string>(),
            LaptopAgreementEngine.DefaultLaptopAgreementEngine());
    }
}
