namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination budget is in its life cycle.</summary>
public enum NominationBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One nomination budget, with what the planners need to act on it.</summary>
public sealed class NominationBudget(string code, string title, string externalRef, int version, int updatedAt, string phone, NominationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Phone { get; set; } = phone;
    public NominationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a nomination budget in status draft.</summary>
    public static NominationBudget NewNominationBudget(string code, string title)
    {
        return new NominationBudget(code, title, "", 0, 0, "", NominationBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationBudgetStatus next)
    {
        switch (Status)
        {
            case NominationBudgetStatus.Draft:
                switch (next)
                {
                    case NominationBudgetStatus.Pending:
                        return true;
                }
                return false;
            case NominationBudgetStatus.Pending:
                switch (next)
                {
                    case NominationBudgetStatus.Active:
                    case NominationBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case NominationBudgetStatus.Active:
                return next == NominationBudgetStatus.Closed;
            case NominationBudgetStatus.Suspended:
                return next == NominationBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(NominationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt * (double)Version, 2);
    }

    public NominationBudget Copy()
    {
        return new NominationBudget(Code, Title, ExternalRef, Version, UpdatedAt, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
