namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped merit cycle budget is in its life cycle.</summary>
public enum CappedMeritCycleBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped merit cycle budget and its current status.</summary>
public sealed class CappedMeritCycleBudget(string code, string name, bool archived, string externalRef, string team, string source, string channel, int priority, string currency, int expiresAt, CappedMeritCycleBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public string Source { get; set; } = source;
    public string Channel { get; set; } = channel;
    public int Priority { get; set; } = priority;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public CappedMeritCycleBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a capped merit cycle budget in status created.</summary>
    public static CappedMeritCycleBudget NewCappedMeritCycleBudget(string code, string name)
    {
        return new CappedMeritCycleBudget(
            code,
            name,
            false,
            "",
            "",
            "",
            "",
            0,
            "",
            0,
            CappedMeritCycleBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMeritCycleBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMeritCycleBudgetStatus next)
    {
        switch (Status)
        {
            case CappedMeritCycleBudgetStatus.Created:
                return next == CappedMeritCycleBudgetStatus.Validated || next == CappedMeritCycleBudgetStatus.Published;
            case CappedMeritCycleBudgetStatus.Validated:
                return next == CappedMeritCycleBudgetStatus.Published || next == CappedMeritCycleBudgetStatus.Retired;
            case CappedMeritCycleBudgetStatus.Published:
                return next == CappedMeritCycleBudgetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedMeritCycleBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped merit cycle budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt + (double)Priority, 2);
    }

    public CappedMeritCycleBudget Copy()
    {
        return new CappedMeritCycleBudget(
            Code,
            Name,
            Archived,
            ExternalRef,
            Team,
            Source,
            Channel,
            Priority,
            Currency,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
