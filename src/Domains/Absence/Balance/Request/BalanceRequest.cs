namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance request is in its life cycle.</summary>
public enum BalanceRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One balance request, with what the planners need to act on it.</summary>
public sealed class BalanceRequest(string code, string name, string notes, int revision, string segment, string department, double budget, int limit, BalanceRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public string Segment { get; set; } = segment;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public int Limit { get; set; } = limit;
    public BalanceRequestStatus Status { get; set; } = status;

    /// <summary>Starts a balance request in status created.</summary>
    public static BalanceRequest NewBalanceRequest(string code, string name)
    {
        return new BalanceRequest(code, name, "", 0, "", "", 0.0, 0, BalanceRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceRequestStatus next)
    {
        switch (Status)
        {
            case BalanceRequestStatus.Created:
                return next == BalanceRequestStatus.Published;
            case BalanceRequestStatus.Validated:
                switch (next)
                {
                    case BalanceRequestStatus.Published:
                    case BalanceRequestStatus.Retired:
                        return true;
                }
                return false;
            case BalanceRequestStatus.Published:
                switch (next)
                {
                    case BalanceRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Budget + (double)Limit, 2);
    }

    public BalanceRequest Copy()
    {
        return new BalanceRequest(Code, Name, Notes, Revision, Segment, Department, Budget, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
