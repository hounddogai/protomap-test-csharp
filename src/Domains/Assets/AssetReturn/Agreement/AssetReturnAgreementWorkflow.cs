namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnAgreementWorkflow(IAssetReturnAgreementStore Store, AssetReturnAgreementValidator Validator, IClock Clock, List<string> Journal, AssetReturnAgreementEngine Engine)
{
    public static AssetReturnAgreementWorkflow NewAssetReturnAgreementWorkflow(IAssetReturnAgreementStore store, IClock clock, Registry flags)
    {
        return new AssetReturnAgreementWorkflow(
            store,
            AssetReturnAgreementRules.DefaultAssetReturnAgreementValidator(),
            clock,
            new List<string>(),
            AssetReturnAgreementEngine.DefaultAssetReturnAgreementEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return agreement {code} not found");
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
