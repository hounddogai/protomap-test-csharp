namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall score is in its life cycle.</summary>
public enum TownHallScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A town hall score. Its code is unique within a tenant.</summary>
public sealed class TownHallScore(string code, string name, string contactEmail, string currency, string title, double amount, TownHallScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Currency { get; set; } = currency;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public TownHallScoreStatus Status { get; set; } = status;

    /// <summary>Starts a town hall score in status draft.</summary>
    public static TownHallScore NewTownHallScore(string code, string name)
    {
        return new TownHallScore(code, name, "", "", "", 0.0, TownHallScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallScoreStatus next)
    {
        switch (Status)
        {
            case TownHallScoreStatus.Draft:
                switch (next)
                {
                    case TownHallScoreStatus.Active:
                    case TownHallScoreStatus.Suspended:
                        return true;
                }
                return false;
            case TownHallScoreStatus.Pending:
                switch (next)
                {
                    case TownHallScoreStatus.Suspended:
                    case TownHallScoreStatus.Closed:
                        return true;
                }
                return false;
            case TownHallScoreStatus.Active:
                return next == TownHallScoreStatus.Suspended;
            case TownHallScoreStatus.Suspended:
                return next == TownHallScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TownHallScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"town hall score"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public TownHallScore Copy()
    {
        return new TownHallScore(Code, Name, ContactEmail, Currency, Title, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
