namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work tier is in its life cycle.</summary>
public enum StatementOfWorkTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a statement of work tier and its current status.</summary>
public sealed class StatementOfWorkTier(string code, string label, string timezone, string description, int priority, int windowDays, StatementOfWorkTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string Description { get; set; } = description;
    public int Priority { get; set; } = priority;
    public int WindowDays { get; set; } = windowDays;
    public StatementOfWorkTierStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work tier in status draft.</summary>
    public static StatementOfWorkTier NewStatementOfWorkTier(string code, string label)
    {
        return new StatementOfWorkTier(code, label, "", "", 0, 0, StatementOfWorkTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkTierStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkTierStatus.Draft:
                return next == StatementOfWorkTierStatus.Active;
            case StatementOfWorkTierStatus.Pending:
                return next == StatementOfWorkTierStatus.Active || next == StatementOfWorkTierStatus.Suspended;
            case StatementOfWorkTierStatus.Active:
                switch (next)
                {
                    case StatementOfWorkTierStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkTierStatus.Suspended:
                switch (next)
                {
                    case StatementOfWorkTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Priority * (double)WindowDays, 2);
    }

    public StatementOfWorkTier Copy()
    {
        return new StatementOfWorkTier(Code, Label, Timezone, Description, Priority, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
